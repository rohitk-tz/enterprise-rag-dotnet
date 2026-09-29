using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectDocumentRepository : IProjectDocumentRepository
{
    private readonly AppDbContext _context;

    public ProjectDocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDocument>> GetByProjectIdAsync(Guid projectId, CancellationToken ct) =>
        await _context.ProjectDocuments
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<ProjectDocument?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _context.ProjectDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<ProjectDocument?> GetByIdForOwnerAsync(Guid id, Guid projectId, string clerkId, CancellationToken ct) =>
        await _context.ProjectDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.ProjectId == projectId && d.ClerkId == clerkId, ct);

    public async Task<ProjectDocument?> GetByS3KeyForOwnerAsync(string s3Key, Guid projectId, string clerkId, CancellationToken ct) =>
        await _context.ProjectDocuments
            .FirstOrDefaultAsync(d => d.S3Key == s3Key && d.ProjectId == projectId && d.ClerkId == clerkId, ct);

    public async Task AddAsync(ProjectDocument document, CancellationToken ct) =>
        await _context.ProjectDocuments.AddAsync(document, ct);

    public void Remove(ProjectDocument document) => _context.ProjectDocuments.Remove(document);

    public async Task<Dictionary<Guid, string>> GetFilenamesByIdsAsync(List<Guid> documentIds, CancellationToken ct) =>
        await _context.ProjectDocuments
            .Where(d => documentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Filename, ct);
}
