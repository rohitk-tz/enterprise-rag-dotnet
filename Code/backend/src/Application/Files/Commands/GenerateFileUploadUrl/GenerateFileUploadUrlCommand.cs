using Contracts.Files;
using MediatR;

namespace Application.Files.Commands.GenerateFileUploadUrl;

public record GenerateFileUploadUrlCommand(
    Guid ProjectId, string Filename, long FileSize, string FileType) : IRequest<GenerateFileUploadUrlResponse>;
