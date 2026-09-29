using System.Net;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class ChatsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ChatsEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetChat_for_nonexistent_chat_returns_404()
    {
        var client = _factory.CreateClientAs("user_test1");

        var response = await client.GetAsync($"/api/chats/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
