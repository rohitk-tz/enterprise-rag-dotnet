using Contracts.Projects;
using MediatR;

namespace Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(string Name, string Description) : IRequest<ProjectResponse>;
