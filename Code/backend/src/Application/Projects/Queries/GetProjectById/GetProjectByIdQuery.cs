using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid ProjectId) : IRequest<ProjectResponse>;
