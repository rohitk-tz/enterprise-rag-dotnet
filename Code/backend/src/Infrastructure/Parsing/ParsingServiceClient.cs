using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Infrastructure.Parsing;

public class ParsingServiceClient : IParsingServiceClient
{
    private readonly HttpClient _httpClient;

    public ParsingServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PartitionAndChunkResult> PartitionAndChunkAsync(
        Stream fileContent, string fileType, string sourceType, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileContent);
        content.Add(streamContent, "file", $"document.{fileType}");
        content.Add(new StringContent(fileType), "file_type");
        content.Add(new StringContent(sourceType), "source_type");

        var response = await _httpClient.PostAsync("/partition-and-chunk", content, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ParsingServiceResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Parsing service returned an empty response.");

        var chunks = payload.Chunks
            .Select(c => new RawChunk(c.Text, c.Tables, c.Images, c.Types, c.PageNumber, c.CharCount))
            .ToList();

        return new PartitionAndChunkResult(chunks, payload.ChunkingMetrics.TotalChunks);
    }

    private record ParsingServiceResponse(
        List<ParsingServiceChunk> Chunks,
        [property: JsonPropertyName("chunking_metrics")] ChunkingMetricsDto ChunkingMetrics);

    private record ParsingServiceChunk(
        string Text,
        List<string> Tables,
        List<string> Images,
        List<string> Types,
        [property: JsonPropertyName("page_number")] int? PageNumber,
        [property: JsonPropertyName("char_count")] int CharCount);

    private record ChunkingMetricsDto([property: JsonPropertyName("total_chunks")] int TotalChunks);
}
