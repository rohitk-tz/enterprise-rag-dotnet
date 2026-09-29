using MediatR;

namespace Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId) : IRequest;
