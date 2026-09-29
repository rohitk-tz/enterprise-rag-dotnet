using System.Net;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class AuthenticationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_request_to_a_protected_endpoint_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/ping");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
