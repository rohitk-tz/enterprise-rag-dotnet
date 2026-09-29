using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Files.Commands.DeleteProjectFile;

public class DeleteProjectFileCommandHandler : IRequestHandler<DeleteProjectFileCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public DeleteProjectFileCommandHandler(
        IProjectRepository projectRepository,
        IProjectDocumentRepository documentRepository,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _documentRepository = documentRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteProjectFileCommand request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var document = await _documentRepository.GetByIdForOwnerAsync(request.FileId, request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Document not found.");

        // Skip the S3 call when s3Key is empty, matching server/routes/files.py:delete_project_file
        // exactly - URL-sourced documents never had an S3 object to begin with.
        if (!string.IsNullOrEmpty(document.S3Key))
        {
            await _fileStorage.DeleteAsync(document.S3Key, ct);
        }

        _documentRepository.Remove(document);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
