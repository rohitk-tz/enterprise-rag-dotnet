using System.Text.Json;
using Application;
using Hangfire;
using Infrastructure;
using Infrastructure.Observability;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Api host");

    var builder = WebApplication.CreateBuilder(args);
    // Serilog owns console output; writeToProviders forwards its events to the OpenTelemetry
    // logger provider below, which attaches them to the active span so they show up in Jaeger.
    // The default providers are cleared so the console doesn't get every line twice.
    builder.Logging.ClearProviders();
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.AddProcessor(new ActivityEventLogProcessor());
    });
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(), writeToProviders: true);

    builder.Services.AddControllers().AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddMvc();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("RagMigration.Api"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            // One span per SQL command Npgsql executes, which covers every EF Core query.
            .AddNpgsql()
            // Stamps the current trace context onto jobs as they're enqueued, so the Workers'
            // job span is parented to the request that created it.
            .AddHangfireInstrumentation()
            // Endpoint comes from OTEL_EXPORTER_OTLP_ENDPOINT (default http://localhost:4317, gRPC).
            .AddOtlpExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter());
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres")
        .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis")
        .AddUrlGroup(new Uri($"{builder.Configuration["ParsingService:BaseUrl"]}/docs"), name: "parsing-service");
    builder.Services.AddApplication();
    builder.Services.AddWebAuthentication(builder.Configuration);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Serilog's request logging must wrap (run outside) ExceptionHandlingMiddleware so it
    // observes the final response status code after exception handling has set it (e.g. 404),
    // rather than the pre-rewrite status of an unhandled exception rethrown through it.
    app.UseSerilogRequestLogging();
    app.UseMiddleware<Api.Middleware.ExceptionHandlingMiddleware>();

    app.UseCors("Default");

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.UseHangfireDashboard("/hangfire");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Api host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { } // exposed for WebApplicationFactory in Integration.Tests
