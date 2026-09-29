using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Documents.Commands.ProcessDocument;

public record ProcessedChunk(string Content, RawChunk Original);

public class ProcessDocumentCommandHandler : IRequestHandler<ProcessDocumentCommand>
{
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IWebCrawlerService _crawler;
    private readonly IParsingServiceClient _parsingClient;
    private readonly ISummarizationService _summarizer;
    private readonly IEmbeddingService _embeddingService;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessDocumentCommandHandler(
        IProjectDocumentRepository documentRepository,
        IFileStorageService fileStorage,
        IWebCrawlerService crawler,
        IParsingServiceClient parsingClient,
        ISummarizationService summarizer,
        IEmbeddingService embeddingService,
        IDocumentChunkRepository chunkRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _fileStorage = fileStorage;
        _crawler = crawler;
        _parsingClient = parsingClient;
        _summarizer = summarizer;
        _embeddingService = embeddingService;
        _chunkRepository = chunkRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessDocumentCommand request, CancellationToken ct)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, ct)
            ?? throw new NotFoundException("Document not found.");

        document.UpdateStatus("partitioning", "{}");
        await _unitOfWork.SaveChangesAsync(ct);

        var isUrlSource = document.SourceType == "url";
        var fileType = isUrlSource ? "html" : document.Filename.Split('.').Last().ToLowerInvariant();

        await using var content = isUrlSource
            ? await _crawler.FetchAsync(document.SourceUrl!, ct)
            : await _fileStorage.DownloadAsync(document.S3Key, ct);

        var parsingResult = await _parsingClient.PartitionAndChunkAsync(content, fileType, document.SourceType, ct);

        document.UpdateStatus("chunking", JsonSerializer.Serialize(new
        {
            chunking = new { total_chunks = parsingResult.TotalChunks }
        }));
        await _unitOfWork.SaveChangesAsync(ct);

        document.UpdateStatus("summarising", "{}");
        await _unitOfWork.SaveChangesAsync(ct);

        var processedChunks = await SummarizeChunksAsync(parsingResult.Chunks, ct);

        document.UpdateStatus("vectorization", "{}");
        await _unitOfWork.SaveChangesAsync(ct);

        var embeddings = await _embeddingService.EmbedBatchAsync(
            processedChunks.Select(p => p.Content).ToList(), ct);

        var entities = processedChunks
            .Zip(embeddings, (processed, embedding) => (processed, embedding))
            .Select((pair, index) => new DocumentChunk(
                Guid.NewGuid(),
                document.Id,
                pair.processed.Content,
                chunkIndex: index,
                pair.processed.Original.PageNumber,
                pair.processed.Content.Length,
                JsonSerializer.Serialize(pair.processed.Original.Types),
                JsonSerializer.Serialize(new
                {
                    text = pair.processed.Original.Text,
                    tables = pair.processed.Original.Tables,
                    images = pair.processed.Original.Images
                }),
                pair.embedding,
                DateTimeOffset.UtcNow));

        await _chunkRepository.AddRangeAsync(entities, ct);

        document.UpdateStatus("completed", "{}");
        await _unitOfWork.SaveChangesAsync(ct);
    }

    internal async Task<List<ProcessedChunk>> SummarizeChunksAsync(IReadOnlyList<RawChunk> chunks, CancellationToken ct)
    {
        var processed = new List<ProcessedChunk>();

        foreach (var chunk in chunks)
        {
            // Only summarize mixed content (tables/images present) - matches
            // server/tasks.py:summarise_chunks exactly; plain-text chunks pass through as-is.
            var content = chunk.Tables.Count > 0 || chunk.Images.Count > 0
                ? await _summarizer.SummarizeAsync(chunk.Text, chunk.Tables, chunk.Images, ct)
                : chunk.Text;

            processed.Add(new ProcessedChunk(content, chunk));
        }

        return processed;
    }
}
