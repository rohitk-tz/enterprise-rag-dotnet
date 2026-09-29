using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IDocumentChunkRepository
{
    Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct);

    Task<List<DocumentChunk>> VectorSearchAsync(
        float[] queryEmbedding, List<Guid> documentIds, decimal matchThreshold, int chunksPerSearch, CancellationToken ct);

    Task<List<DocumentChunk>> KeywordSearchAsync(
        string queryText, List<Guid> documentIds, int chunksPerSearch, CancellationToken ct);
}
