using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Project>> GetAllForOwnerAsync(string clerkId, CancellationToken ct) =>
        await _context.Projects
            .Where(p => p.ClerkId == clerkId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<Project?> GetByIdForOwnerAsync(Guid id, string clerkId, CancellationToken ct) =>
        await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.ClerkId == clerkId, ct);

    public async Task AddAsync(Project project, CancellationToken ct) =>
        await _context.Projects.AddAsync(project, ct);

    public void Remove(Project project) => _context.Projects.Remove(project);
}
