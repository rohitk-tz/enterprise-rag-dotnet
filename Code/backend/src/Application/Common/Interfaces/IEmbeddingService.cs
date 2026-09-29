namespace Application.Common.Interfaces;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct);
    Task<float[]> EmbedSingleAsync(string text, CancellationToken ct);
}
