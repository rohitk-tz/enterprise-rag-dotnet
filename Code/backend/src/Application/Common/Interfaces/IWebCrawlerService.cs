namespace Application.Common.Interfaces;

public interface IWebCrawlerService
{
    Task<Stream> FetchAsync(string url, CancellationToken ct);
}
