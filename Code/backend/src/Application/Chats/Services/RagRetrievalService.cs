using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Services;

public class RagRetrievalService : IRagRetrievalService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IQueryVariationService _queryVariationService;
    private readonly IRerankingService _rerankingService;
    private readonly ILogger<RagRetrievalService> _logger;

    public RagRetrievalService(
        IEmbeddingService embeddingService,
        IDocumentChunkRepository chunkRepository,
        IQueryVariationService queryVariationService,
        IRerankingService rerankingService,
        ILogger<RagRetrievalService> logger)
    {
        _embeddingService = embeddingService;
        _chunkRepository = chunkRepository;
        _queryVariationService = queryVariationService;
        _rerankingService = rerankingService;
        _logger = logger;
    }

    public async Task<List<DocumentChunk>> RetrieveAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct)
    {
        // isBasic is derived per-arm below, alongside the search call it belongs to, so the
        // strategy -> behavior mapping only ever lives in one place. "basic" is any strategy
        // that isn't one of the three named ones; it falls through to plain vector search and,
        // unlike the others, was never truncated before this feature existed, so it stays
        // untruncated here too.
        var (chunks, isBasic) = settings.RagStrategy switch
        {
            "hybrid" => (await HybridSearchAsync(query, documentIds, settings, ct), false),
            "multi-query-vector" => (await MultiQueryVectorSearchAsync(query, documentIds, settings, ct), false),
            "multi-query-hybrid" => (await MultiQueryHybridSearchAsync(query, documentIds, settings, ct), false),
            _ => (await VectorSearchAsync(query, documentIds, settings, ct), true),
        };

        _logger.LogInformation("{Strategy} search resulted in {Count} chunks", settings.RagStrategy, chunks.Count);

        if (chunks.Count == 0)
        {
            return chunks;
        }

        if (settings.RerankingEnabled)
        {
            chunks = await _rerankingService.RerankAsync(query, chunks, settings.RerankingModel, ct);
        }

        return isBasic ? chunks : chunks.Take(settings.FinalContextSize).ToList();
    }

    private async Task<List<DocumentChunk>> VectorSearchAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct)
    {
        var embedding = await _embeddingService.EmbedSingleAsync(query, ct);
        return await _chunkRepository.VectorSearchAsync(
            embedding, documentIds, settings.SimilarityThreshold, settings.ChunksPerSearch, ct);
    }

    private async Task<List<DocumentChunk>> HybridSearchAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct)
    {
        var vectorChunks = await VectorSearchAsync(query, documentIds, settings, ct);
        var keywordChunks = await _chunkRepository.KeywordSearchAsync(query, documentIds, settings.ChunksPerSearch, ct);

        _logger.LogInformation(
            "Hybrid search: vector returned {VectorCount} chunks, keyword returned {KeywordCount} chunks",
            vectorChunks.Count, keywordChunks.Count);

        return RrfFusion.Fuse(
            new List<List<DocumentChunk>> { vectorChunks, keywordChunks },
            new List<double> { (double)settings.VectorWeight, (double)settings.KeywordWeight });
    }

    private async Task<List<DocumentChunk>> MultiQueryVectorSearchAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct)
    {
        var queries = await _queryVariationService.GenerateVariationsAsync(query, settings.NumberOfQueries, ct);

        var resultSets = new List<List<DocumentChunk>>();
        foreach (var variation in queries)
        {
            resultSets.Add(await VectorSearchAsync(variation, documentIds, settings, ct));
        }

        return RrfFusion.Fuse(resultSets);
    }

    private async Task<List<DocumentChunk>> MultiQueryHybridSearchAsync(
        string query, List<Guid> documentIds, ProjectSettings settings, CancellationToken ct)
    {
        var queries = await _queryVariationService.GenerateVariationsAsync(query, settings.NumberOfQueries, ct);

        var resultSets = new List<List<DocumentChunk>>();
        foreach (var variation in queries)
        {
            resultSets.Add(await HybridSearchAsync(variation, documentIds, settings, ct));
        }

        return RrfFusion.Fuse(resultSets);
    }
}
