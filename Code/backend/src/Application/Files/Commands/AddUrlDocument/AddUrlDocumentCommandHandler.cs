using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Files;
using Domain.Entities;
using Hangfire;
using MediatR;

namespace Application.Files.Commands.AddUrlDocument;

public class AddUrlDocumentCommandHandler : IRequestHandler<AddUrlDocumentCommand, ProjectDocumentResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public AddUrlDocumentCommandHandler(
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

    public async Task<ProjectDocumentResponse> Handle(AddUrlDocumentCommand request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var document = new ProjectDocument(
            Guid.NewGuid(), request.ProjectId, request.Url, s3Key: "", fileSize: 0, fileType: "text/html",
            processingStatus: "queued", taskId: null, sourceType: "url", sourceUrl: request.Url,
            processingDetailsJson: "{}", _currentUser.ClerkId, DateTimeOffset.UtcNow);

        await _documentRepository.AddAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var jobId = _backgroundJobClient.Enqueue<IDocumentProcessingJob>(job => job.ProcessAsync(document.Id));
        document.SetTaskId(jobId);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ProjectDocumentResponse(
            document.Id, document.ProjectId, document.Filename, document.FileSize, document.FileType,
            document.ProcessingStatus, document.SourceType, document.SourceUrl, document.CreatedAt);
    }
}
