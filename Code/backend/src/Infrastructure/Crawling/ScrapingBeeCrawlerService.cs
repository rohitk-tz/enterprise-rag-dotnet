using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Crawling;

public class ScrapingBeeCrawlerService : IWebCrawlerService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ScrapingBeeCrawlerService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ScrapingBee:ApiKey"]
            ?? throw new InvalidOperationException("ScrapingBee:ApiKey is not configured.");
    }

    public async Task<Stream> FetchAsync(string url, CancellationToken ct)
    {
        var requestUri = $"https://app.scrapingbee.com/api/v1/?api_key={_apiKey}&url={Uri.EscapeDataString(url)}";
        var response = await _httpClient.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(ct);
    }
}
