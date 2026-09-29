using System.Net.Http.Json;
using System.Text.Json;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Ai;

public class CohereRerankService : IRerankingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<CohereRerankService> _logger;

    public CohereRerankService(HttpClient httpClient, ILogger<CohereRerankService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<DocumentChunk>> RerankAsync(
        string query, List<DocumentChunk> chunks, string model, CancellationToken ct)
    {
        try
        {
            var request = new CohereRerankRequest(model, query, chunks.Select(c => c.Content).ToList(), chunks.Count);
            var response = await _httpClient.PostAsJsonAsync("/v2/rerank", request, JsonOptions, ct);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<CohereRerankResponse>(JsonOptions, ct)
                ?? throw new InvalidOperationException("Cohere rerank returned an empty response.");

            return payload.Results
                .OrderByDescending(r => r.RelevanceScore)
                .Select(r => chunks[r.Index])
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reranking failed; falling back to unreranked order");
            return chunks;
        }
    }

    private record CohereRerankRequest(string Model, string Query, List<string> Documents, int TopN);
    private record CohereRerankResponse(List<CohereRerankResult> Results);
    private record CohereRerankResult(int Index, double RelevanceScore);
}
