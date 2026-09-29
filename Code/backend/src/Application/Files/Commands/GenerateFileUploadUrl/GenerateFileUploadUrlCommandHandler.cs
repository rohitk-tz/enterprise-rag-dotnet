using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Files;
using Domain.Entities;
using MediatR;

namespace Application.Files.Commands.GenerateFileUploadUrl;

public class GenerateFileUploadUrlCommandHandler
    : IRequestHandler<GenerateFileUploadUrlCommand, GenerateFileUploadUrlResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public GenerateFileUploadUrlCommandHandler(
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

    public async Task<GenerateFileUploadUrlResponse> Handle(GenerateFileUploadUrlCommand request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var fileId = Guid.NewGuid();
        var extension = request.FileType.Split('/').Last();
        var s3Key = $"projects/{request.ProjectId}/documents/{fileId}/{fileId}.{extension}";

        var document = new ProjectDocument(
            fileId, request.ProjectId, request.Filename, s3Key, (int)request.FileSize, request.FileType,
            "uploading", null, "file", null, "{}", _currentUser.ClerkId, DateTimeOffset.UtcNow);
        await _documentRepository.AddAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var uploadUrl = _fileStorage.GeneratePresignedUploadUrl(s3Key, request.FileType);

        return new GenerateFileUploadUrlResponse(uploadUrl, s3Key);
    }
}
