using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Files;
using MediatR;

namespace Application.Files.Queries.GetDocumentChunks;

public class GetDocumentChunksQueryHandler : IRequestHandler<GetDocumentChunksQuery, List<DocumentChunkResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetDocumentChunksQueryHandler(
        IProjectRepository projectRepository,
        IProjectDocumentRepository documentRepository,
        IDocumentChunkRepository chunkRepository,
        ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _documentRepository = documentRepository;
        _chunkRepository = chunkRepository;
        _currentUser = currentUser;
    }

    public async Task<List<DocumentChunkResponse>> Handle(GetDocumentChunksQuery request, CancellationToken ct)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found or access denied");
        }

        var document = await _documentRepository.GetByIdAsync(request.FileId, ct);
        if (document is null || document.ProjectId != request.ProjectId)
        {
            throw new NotFoundException("Document not found");
        }

        var chunks = await _chunkRepository.GetByDocumentIdAsync(request.FileId, ct);

        return chunks
            .Select(c => new DocumentChunkResponse(c.Id, c.DocumentId, c.Content, c.ChunkIndex, c.PageNumber, c.CharCount, c.CreatedAt))
            .ToList();
    }
}
