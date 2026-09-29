using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllForOwnerAsync(string clerkId, CancellationToken ct);
    Task<Project?> GetByIdForOwnerAsync(Guid id, string clerkId, CancellationToken ct);
    Task AddAsync(Project project, CancellationToken ct);
    void Remove(Project project);
}
