using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<List<ProjectResponse>>;
