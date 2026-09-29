using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public class ProjectsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProjectsEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProjects_without_auth_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/projects");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProjectById_for_nonexistent_project_returns_404()
    {
        var client = _factory.CreateClientAs("user_test1");

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectById_owned_by_a_different_user_returns_404()
    {
        // Seed a project that genuinely exists, owned by "owner", directly via the DbContext -
        // there is no HTTP create-project endpoint yet, and this exercises the exact scenario
        // GetByIdForOwnerAsync's ownership filter is meant to guard: a real row exists, just not
        // for the caller's clerk_id. Unlike a nonexistent-id lookup, ASP.NET Core routing cannot
        // produce this 404 on its own, so a pass here can only come from the real
        // NotFoundException -> exception-middleware path.
        var ownerClerkId = $"user_owner_{Guid.NewGuid():N}";
        Guid projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(Guid.NewGuid(), ownerClerkId, DateTimeOffset.UtcNow));
            var project = new Project(Guid.NewGuid(), "Owner's Project", null, ownerClerkId, DateTimeOffset.UtcNow);
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        var otherClient = _factory.CreateClientAs($"user_different_{Guid.NewGuid():N}");

        var response = await otherClient.GetAsync($"/api/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectChats_for_nonexistent_project_returns_404()
    {
        var client = _factory.CreateClientAs("user_test1");

        // A nonexistent project ID still exercises the ownership-check path (404),
        // proving the route and MediatR wiring are correct without needing seeded data.
        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/chats");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectSettings_for_nonexistent_project_returns_404()
    {
        var client = ((TestWebApplicationFactory)_factory).CreateClientAs("user_test1");

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/settings");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectSettings_for_project_with_settings_returns_settings()
    {
        var clerkId = "user_test1";
        var projectId = Guid.NewGuid();
        var settingsId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(Guid.NewGuid(), clerkId, DateTimeOffset.UtcNow));
            db.Projects.Add(new Project(projectId, "Test Project", null, clerkId, DateTimeOffset.UtcNow));
            db.ProjectSettings.Add(new ProjectSettings(
                settingsId, projectId, "text-embedding-3-small", "BM25", "standard",
                10, 2000, 0.7m, 3, true, "rerank-model", 0.5m, 0.5m, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = ((TestWebApplicationFactory)_factory).CreateClientAs(clerkId);

        var response = await client.GetAsync($"/api/projects/{projectId}/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var root = jsonDoc.RootElement;

        root.GetProperty("message").GetString().Should().Be("Project settings retrieved successfully");
        var data = root.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(settingsId);
        data.GetProperty("project_id").GetGuid().Should().Be(projectId);
        data.GetProperty("embedding_model").GetString().Should().Be("text-embedding-3-small");
        data.GetProperty("rag_strategy").GetString().Should().Be("BM25");
    }

    [Fact]
    public async Task GetProjectChats_with_chats_returns_chats_with_snake_case_properties()
    {
        var clerkId = $"user_chats_test_{Guid.NewGuid():N}";
        var projectId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(Guid.NewGuid(), clerkId, DateTimeOffset.UtcNow));
            db.Projects.Add(new Project(projectId, "Test Project", null, clerkId, DateTimeOffset.UtcNow));
            db.Chats.Add(new Chat(chatId, "Test Chat", projectId, clerkId, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClientAs(clerkId);

        var response = await client.GetAsync($"/api/projects/{projectId}/chats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var root = jsonDoc.RootElement;

        root.GetProperty("message").GetString().Should().Be("Chats retrieved successfully");
        var data = root.GetProperty("data");
        data.GetArrayLength().Should().Be(1);
        var chat = data[0];
        chat.GetProperty("id").GetGuid().Should().Be(chatId);
        chat.GetProperty("title").GetString().Should().Be("Test Chat");
        chat.GetProperty("project_id").GetGuid().Should().Be(projectId);
        chat.GetProperty("created_at").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetChatWithMessages_returns_chat_with_messages_using_snake_case_properties()
    {
        var clerkId = $"user_msg_test_{Guid.NewGuid():N}";
        var projectId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(Guid.NewGuid(), clerkId, DateTimeOffset.UtcNow));
            db.Projects.Add(new Project(projectId, "Test Project", null, clerkId, DateTimeOffset.UtcNow));
            db.Chats.Add(new Chat(chatId, "Test Chat", projectId, clerkId, DateTimeOffset.UtcNow));
            db.Messages.Add(new Message(
                messageId, "Test message content", "user", chatId, clerkId,
                "[]", null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClientAs(clerkId);

        var response = await client.GetAsync($"/api/chats/{chatId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var root = jsonDoc.RootElement;

        root.GetProperty("message").GetString().Should().Be("Chat retrieved successfully");
        var data = root.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(chatId);
        data.GetProperty("title").GetString().Should().Be("Test Chat");
        data.GetProperty("project_id").GetGuid().Should().Be(projectId);
        data.GetProperty("created_at").ValueKind.Should().NotBe(JsonValueKind.Null);

        var messages = data.GetProperty("messages");
        messages.GetArrayLength().Should().Be(1);
        var message = messages[0];
        message.GetProperty("id").GetGuid().Should().Be(messageId);
        message.GetProperty("content").GetString().Should().Be("Test message content");
        message.GetProperty("role").GetString().Should().Be("user");
        message.GetProperty("chat_id").GetGuid().Should().Be(chatId);
        message.GetProperty("created_at").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetChatWithMessages_deserializes_citations_with_snake_case_properties()
    {
        var clerkId = $"user_citations_test_{Guid.NewGuid():N}";
        var projectId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var citationsJson = """[{"chunk_id":"c1","document_id":"d1","filename":"report.pdf","page":3}]""";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(Guid.NewGuid(), clerkId, DateTimeOffset.UtcNow));
            db.Projects.Add(new Project(projectId, "Test Project", null, clerkId, DateTimeOffset.UtcNow));
            db.Chats.Add(new Chat(chatId, "Test Chat", projectId, clerkId, DateTimeOffset.UtcNow));
            db.Messages.Add(new Message(
                messageId, "AI response with citations", "assistant", chatId, clerkId,
                citationsJson, null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClientAs(clerkId);

        var response = await client.GetAsync($"/api/chats/{chatId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var root = jsonDoc.RootElement;

        var messages = root.GetProperty("data").GetProperty("messages");
        messages.GetArrayLength().Should().Be(1);
        var message = messages[0];

        var citations = message.GetProperty("citations");
        citations.GetArrayLength().Should().Be(1);
        var citation = citations[0];

        citation.GetProperty("chunk_id").GetString().Should().Be("c1");
        citation.GetProperty("document_id").GetString().Should().Be("d1");
        citation.GetProperty("filename").GetString().Should().Be("report.pdf");
        citation.GetProperty("page").GetInt32().Should().Be(3);
    }
}
