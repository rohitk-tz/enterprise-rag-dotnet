using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Files;
using Hangfire;
using MediatR;

namespace Application.Files.Commands.ConfirmFileUpload;

public class ConfirmFileUploadCommandHandler : IRequestHandler<ConfirmFileUploadCommand, ProjectDocumentResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public ConfirmFileUploadCommandHandler(
        IProjectRepository projectRepository,
        IProjectDocumentRepository documentRepository,
        IBackgroundJobClient backgroundJobClient,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _documentRepository = documentRepository;
        _backgroundJobClient = backgroundJobClient;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProjectDocumentResponse> Handle(ConfirmFileUploadCommand request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var document = await _documentRepository.GetByS3KeyForOwnerAsync(request.S3Key, request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Document not found.");

        var jobId = _backgroundJobClient.Enqueue<IDocumentProcessingJob>(job => job.ProcessAsync(document.Id));
        document.SetTaskId(jobId);
        document.UpdateStatus("queued", "{}");

        await _unitOfWork.SaveChangesAsync(ct);

        return new ProjectDocumentResponse(
            document.Id, document.ProjectId, document.Filename, document.FileSize, document.FileType,
            document.ProcessingStatus, document.SourceType, document.SourceUrl, document.CreatedAt);
    }
}
