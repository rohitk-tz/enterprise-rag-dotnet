using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Projects;
using MediatR;

namespace Application.Projects.Queries.GetProjectSettings;

public class GetProjectSettingsQueryHandler : IRequestHandler<GetProjectSettingsQuery, ProjectSettingsResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectSettingsRepository _settingsRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetProjectSettingsQueryHandler(
        IProjectRepository projectRepository, IProjectSettingsRepository settingsRepository, ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _settingsRepository = settingsRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectSettingsResponse> Handle(GetProjectSettingsQuery request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var settings = await _settingsRepository.GetByProjectIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException("Project settings not found.");

        return new ProjectSettingsResponse(
            settings.Id, settings.ProjectId, settings.EmbeddingModel, settings.RagStrategy,
            settings.AgentType, settings.ChunksPerSearch, settings.FinalContextSize,
            settings.SimilarityThreshold, settings.NumberOfQueries, settings.RerankingEnabled,
            settings.RerankingModel, settings.VectorWeight, settings.KeywordWeight);
    }
}
