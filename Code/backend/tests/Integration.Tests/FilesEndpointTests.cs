using System.Net;
using FluentAssertions;
using Xunit;

namespace Integration.Tests;

public class FilesEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public FilesEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProjectFiles_for_nonexistent_project_returns_404()
    {
        var client = _factory.CreateClientAs("user_test1");

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/files");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDocumentChunks_for_nonexistent_project_returns_404()
    {
        var client = _factory.CreateClientAs("user_test1");

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/files/{Guid.NewGuid()}/chunks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
