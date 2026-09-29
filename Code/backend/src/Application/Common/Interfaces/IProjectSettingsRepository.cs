using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProjectSettingsRepository
{
    Task<ProjectSettings?> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task AddAsync(ProjectSettings settings, CancellationToken ct);
}
