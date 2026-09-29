using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectSettingsRepository : IProjectSettingsRepository
{
    private readonly AppDbContext _context;

    public ProjectSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectSettings?> GetByProjectIdAsync(Guid projectId, CancellationToken ct) =>
        await _context.ProjectSettings.FirstOrDefaultAsync(s => s.ProjectId == projectId, ct);

    public async Task AddAsync(ProjectSettings settings, CancellationToken ct) =>
        await _context.ProjectSettings.AddAsync(settings, ct);
}
