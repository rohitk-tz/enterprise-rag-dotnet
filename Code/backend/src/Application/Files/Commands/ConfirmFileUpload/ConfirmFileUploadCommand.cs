using Contracts.Files;
using MediatR;

namespace Application.Files.Commands.ConfirmFileUpload;

public record ConfirmFileUploadCommand(Guid ProjectId, string S3Key) : IRequest<ProjectDocumentResponse>;
