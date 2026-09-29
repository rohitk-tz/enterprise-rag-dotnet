using Application.Common;
using Application.Common.Interfaces;
using Contracts.Projects;
using Domain.Entities;
using MediatR;

namespace Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IProjectSettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProjectResponse> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var project = new Project(Guid.NewGuid(), request.Name, request.Description, _currentUser.ClerkId, DateTimeOffset.UtcNow);
        await _projectRepository.AddAsync(project, ct);

        // Hardcoded defaults matching server/routes/projects.py:create_project exactly.
        var settings = new ProjectSettings(
            Guid.NewGuid(), project.Id, "text-embedding-3-large", "basic", "agentic",
            chunksPerSearch: 10, finalContextSize: 5, similarityThreshold: 0.3m,
            numberOfQueries: 5, rerankingEnabled: true, rerankingModel: "rerank-english-v3.0",
            vectorWeight: 0.7m, keywordWeight: 0.3m, DateTimeOffset.UtcNow);
        await _settingsRepository.AddAsync(settings, ct);

        // Single SaveChangesAsync call persists both inserts in one transaction - if either
        // fails, neither is committed, matching (and improving on) the Python code's explicit
        // rollback-by-delete pattern in server/routes/projects.py:create_project.
        await _unitOfWork.SaveChangesAsync(ct);

        return new ProjectResponse(project.Id, project.Name, project.Description, project.CreatedAt);
    }
}
