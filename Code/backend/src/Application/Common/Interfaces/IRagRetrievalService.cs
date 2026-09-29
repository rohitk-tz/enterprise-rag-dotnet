using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRagRetrievalService
{
    Task<List<DocumentChunk>> RetrieveAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct);
}
