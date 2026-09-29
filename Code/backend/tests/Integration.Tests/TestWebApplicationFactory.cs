using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Integration.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Apply migrations to the test database
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override the connection string to use the test container BEFORE AddInfrastructure runs
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "ConnectionStrings:Postgres", _postgres.GetConnectionString() }
            };
            config.AddInMemoryCollection(inMemoryConfig);
        });

        builder.ConfigureServices(services =>
        {
            // NOTE: intentionally calling the parameterless AddAuthentication() overload here
            // (not AddAuthentication(TestAuthHandler.SchemeName)). Infrastructure's
            // AddWebAuthentication already set the app's DefaultScheme to
            // JwtBearerDefaults.AuthenticationScheme via AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
            // Calling AddAuthentication(string) again here would overwrite that DefaultScheme to
            // "Test" globally, which would break anonymous requests: [Authorize] without an
            // explicit scheme falls back to the DefaultAuthenticateScheme, and anonymous requests
            // must still be challenged by the real JwtBearer handler (returning 401) rather than
            // by the Test handler. Using AddAuthentication() only registers the "Test" scheme
            // alongside the existing JwtBearer scheme, without touching the default.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // [Authorize] with no explicit Policy/AuthenticationSchemes uses AuthorizationOptions.DefaultPolicy.
            // Overriding it here to accept either scheme lets a request carrying X-Test-ClerkId
            // authenticate via "Test" while a request carrying a real bearer token still
            // authenticates via JwtBearer - and a request with neither still fails authentication
            // (401), preserving the anonymous-request behavior verified by AuthenticationTests.
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(
                        TestAuthHandler.SchemeName, JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }

    public HttpClient CreateClientAs(string clerkId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestClerkIdHeader, clerkId);
        return client;
    }
}
