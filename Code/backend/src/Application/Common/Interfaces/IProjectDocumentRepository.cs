using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProjectDocumentRepository
{
    Task<List<ProjectDocument>> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task<ProjectDocument?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ProjectDocument?> GetByIdForOwnerAsync(Guid id, Guid projectId, string clerkId, CancellationToken ct);
    Task<ProjectDocument?> GetByS3KeyForOwnerAsync(string s3Key, Guid projectId, string clerkId, CancellationToken ct);
    Task AddAsync(ProjectDocument document, CancellationToken ct);
    void Remove(ProjectDocument document);
    Task<Dictionary<Guid, string>> GetFilenamesByIdsAsync(List<Guid> documentIds, CancellationToken ct);
}
