using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponse>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetProjectByIdQueryHandler(IProjectRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ProjectResponse> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await _repository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        return new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAt);
    }
}
