using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRerankingService
{
    Task<List<DocumentChunk>> RerankAsync(string query, List<DocumentChunk> chunks, string model, CancellationToken ct);
}
