using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class RootEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public RootEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_returns_the_welcome_message()
    {
        var client = _factory.CreateClient();

        var response = await client.GetFromJsonAsync<Dictionary<string, string>>("/");

        response!["message"].Should().Be("Welcome to My engineering API!");
    }
}
