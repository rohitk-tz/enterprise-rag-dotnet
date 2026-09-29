using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;

namespace Infrastructure.Ai;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private const int BatchSize = 10;
    private const int Dimensions = 1536;

    private readonly EmbeddingClient _client;

    public OpenAiEmbeddingService(IConfiguration configuration)
    {
        _client = new EmbeddingClient("text-embedding-3-large", configuration["OpenAI:ApiKey"]);
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        var results = new List<float[]>();
        var options = new EmbeddingGenerationOptions { Dimensions = Dimensions };

        for (var i = 0; i < texts.Count; i += BatchSize)
        {
            var batch = texts.Skip(i).Take(BatchSize).ToList();
            var response = await _client.GenerateEmbeddingsAsync(batch, options, ct);
            results.AddRange(response.Value.Select(e => e.ToFloats().ToArray()));
        }

        return results;
    }

    public async Task<float[]> EmbedSingleAsync(string text, CancellationToken ct)
    {
        var options = new EmbeddingGenerationOptions { Dimensions = Dimensions };
        var response = await _client.GenerateEmbeddingAsync(text, options, ct);
        return response.Value.ToFloats().ToArray();
    }
}
