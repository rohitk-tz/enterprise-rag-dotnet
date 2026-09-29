using MediatR;

namespace Application.Files.Commands.DeleteProjectFile;

public record DeleteProjectFileCommand(Guid ProjectId, Guid FileId) : IRequest;
