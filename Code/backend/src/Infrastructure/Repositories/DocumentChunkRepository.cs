using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Infrastructure.Repositories;

public class DocumentChunkRepository : IDocumentChunkRepository
{
    private readonly AppDbContext _context;

    public DocumentChunkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct) =>
        await _context.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct) =>
        await _context.DocumentChunks.AddRangeAsync(chunks, ct);

    public async Task<List<DocumentChunk>> VectorSearchAsync(
        float[] queryEmbedding, List<Guid> documentIds, decimal matchThreshold, int chunksPerSearch, CancellationToken ct)
    {
        var vector = new Vector(queryEmbedding);
        var threshold = (double)matchThreshold;
        var ids = documentIds.ToArray();

        return await _context.DocumentChunks
            .FromSqlInterpolated($"SELECT * FROM vector_search_document_chunks({vector}, {ids}, {threshold}, {chunksPerSearch})")
            .ToListAsync(ct);
    }

    public async Task<List<DocumentChunk>> KeywordSearchAsync(
        string queryText, List<Guid> documentIds, int chunksPerSearch, CancellationToken ct)
    {
        var ids = documentIds.ToArray();

        return await _context.DocumentChunks
            .FromSqlInterpolated($"SELECT * FROM keyword_search_document_chunks({queryText}, {ids}, {chunksPerSearch})")
            .ToListAsync(ct);
    }
}
