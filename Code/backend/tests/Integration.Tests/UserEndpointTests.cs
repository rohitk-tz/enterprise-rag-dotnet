using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class UserEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public UserEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateUser_with_invalid_event_type_returns_400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/create-user", new { type = "user.updated", data = new { id = "user_2abc" } });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
