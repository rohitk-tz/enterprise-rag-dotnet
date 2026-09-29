using System.Net;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class ExceptionHandlingTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ExceptionHandlingTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NotFoundException_maps_to_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/ping/throws-not-found");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForbiddenException_maps_to_403()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/ping/throws-forbidden");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
