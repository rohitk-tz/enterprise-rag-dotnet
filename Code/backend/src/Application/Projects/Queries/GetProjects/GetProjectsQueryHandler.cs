using Application.Common;
using Application.Common.Interfaces;
using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectResponse>>
{
    private readonly IProjectRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetProjectsQueryHandler(IProjectRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<ProjectResponse>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var projects = await _repository.GetAllForOwnerAsync(_currentUser.ClerkId, ct);

        return projects
            .Select(p => new ProjectResponse(p.Id, p.Name, p.Description, p.CreatedAt))
            .ToList();
    }
}
