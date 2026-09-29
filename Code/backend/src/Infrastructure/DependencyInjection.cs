using Amazon.S3;
using Application.Common;
using Application.Common.Interfaces;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Infrastructure.Auth;
using Infrastructure.Jobs;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.UseVector()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<Application.Common.Interfaces.IChatRepository, Repositories.ChatRepository>();
        services.AddScoped<Application.Common.Interfaces.IProjectSettingsRepository, Repositories.ProjectSettingsRepository>();
        services.AddScoped<Application.Common.Interfaces.IProjectDocumentRepository, Repositories.ProjectDocumentRepository>();
        services.AddScoped<Application.Common.Interfaces.IDocumentChunkRepository, Repositories.DocumentChunkRepository>();
        services.AddScoped<Application.Common.Interfaces.IMessageRepository, Repositories.MessageRepository>();
        services.AddScoped<Application.Common.Interfaces.IUnitOfWork, UnitOfWork>();
        services.AddScoped<Application.Common.Interfaces.IUserRepository, Repositories.UserRepository>();
        services.AddScoped<Application.Common.Interfaces.IDocumentProcessingJob, DocumentProcessingJob>();

        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(new AmazonS3Config
        {
            ServiceURL = configuration["AWS:ServiceURL"],
            ForcePathStyle = true
        }));
        services.AddScoped<Application.Common.Interfaces.IFileStorageService, Storage.S3FileStorageService>();

        services.AddHttpClient<Application.Common.Interfaces.IWebCrawlerService, Crawling.ScrapingBeeCrawlerService>()
            .AddStandardResilienceHandler();
        services.AddHttpClient<Application.Common.Interfaces.IParsingServiceClient, Parsing.ParsingServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ParsingService:BaseUrl"]!);
            // The request body streams the source file once; a retry can't safely re-send it after
            // a failed/timed-out attempt has already started reading the stream, so this client
            // intentionally skips AddStandardResilienceHandler()'s retry/circuit-breaker bundle and
            // just gets a generous timeout for OCR-heavy PDF partitioning instead of the default 100s.
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddSingleton(sp =>
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4.1", apiKey: configuration["OpenAI:ApiKey"]!, serviceId: "summarization");
            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4o", apiKey: configuration["OpenAI:ApiKey"]!, serviceId: "chat");
            return builder.Build();
        });
        services.AddScoped<Application.Common.Interfaces.ISummarizationService, Ai.SemanticKernelSummarizationService>();
        services.AddScoped<Application.Common.Interfaces.IEmbeddingService, Ai.OpenAiEmbeddingService>();
        services.AddScoped<Application.Common.Interfaces.IChatAnswerService, Ai.SemanticKernelChatAnswerService>();
        services.AddScoped<Application.Common.Interfaces.IQueryVariationService, Ai.SemanticKernelQueryVariationService>();

        services.AddHttpClient<Application.Common.Interfaces.IRerankingService, Ai.CohereRerankService>(client =>
        {
            client.BaseAddress = new Uri("https://api.cohere.com");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration["Cohere:ApiKey"]);
        }).AddStandardResilienceHandler();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString)
            && !redisConnectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
        {
            // Without this, StackExchange.Redis's ConnectionMultiplexer.Connect (called internally
            // by UseRedisStorage) throws RedisConnectionException synchronously if Redis is not
            // reachable at startup, crashing the entire API/Worker host. abortConnect=false lets
            // the multiplexer come up in a disconnected state and retry in the background instead,
            // so a transient Redis outage doesn't take down the whole process.
            redisConnectionString += ",abortConnect=false";
        }

        services.AddHangfire(config => config
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(redisConnectionString));

        return services;
    }
}
