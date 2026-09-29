using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Integration.Tests;

public class SchemaMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task Migrations_create_expected_tables_and_search_functions()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(), o => o.UseVector())
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var tableNames = await context.Database
            .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        tableNames.Should().Contain(new[]
        {
            "users", "projects", "project_settings", "project_documents",
            "document_chunks", "chats", "messages"
        });

        var functionNames = await context.Database
            .SqlQueryRaw<string>("SELECT routine_name FROM information_schema.routines WHERE routine_schema = 'public'")
            .ToListAsync();

        functionNames.Should().Contain(new[]
        {
            "vector_search_document_chunks", "keyword_search_document_chunks"
        });
    }
}
