using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Files;
using MediatR;

namespace Application.Files.Queries.GetProjectFiles;

public class GetProjectFilesQueryHandler : IRequestHandler<GetProjectFilesQuery, List<ProjectDocumentResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetProjectFilesQueryHandler(
        IProjectRepository projectRepository, IProjectDocumentRepository documentRepository, ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _documentRepository = documentRepository;
        _currentUser = currentUser;
    }

    public async Task<List<ProjectDocumentResponse>> Handle(GetProjectFilesQuery request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var documents = await _documentRepository.GetByProjectIdAsync(request.ProjectId, ct);

        return documents
            .Select(d => new ProjectDocumentResponse(
                d.Id, d.ProjectId, d.Filename, d.FileSize, d.FileType,
                d.ProcessingStatus, d.SourceType, d.SourceUrl, d.CreatedAt))
            .ToList();
    }
}
