using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjectSettings;

public record GetProjectSettingsQuery(Guid ProjectId) : IRequest<ProjectSettingsResponse>;
