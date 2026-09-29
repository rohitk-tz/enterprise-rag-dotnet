using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Projects;
using MediatR;

namespace Application.Projects.Commands.UpdateProjectSettings;

public class UpdateProjectSettingsCommandHandler : IRequestHandler<UpdateProjectSettingsCommand, ProjectSettingsResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public UpdateProjectSettingsCommandHandler(
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

    public async Task<ProjectSettingsResponse> Handle(UpdateProjectSettingsCommand request, CancellationToken ct)
    {
        var project = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var settings = await _settingsRepository.GetByProjectIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException("Project settings not found.");

        settings.Update(
            request.EmbeddingModel, request.RagStrategy, request.AgentType, request.ChunksPerSearch,
            request.FinalContextSize, request.SimilarityThreshold, request.NumberOfQueries,
            request.RerankingEnabled, request.RerankingModel, request.VectorWeight, request.KeywordWeight);

        await _unitOfWork.SaveChangesAsync(ct);

        return new ProjectSettingsResponse(
            settings.Id, settings.ProjectId, settings.EmbeddingModel, settings.RagStrategy,
            settings.AgentType, settings.ChunksPerSearch, settings.FinalContextSize,
            settings.SimilarityThreshold, settings.NumberOfQueries, settings.RerankingEnabled,
            settings.RerankingModel, settings.VectorWeight, settings.KeywordWeight);
    }
}
