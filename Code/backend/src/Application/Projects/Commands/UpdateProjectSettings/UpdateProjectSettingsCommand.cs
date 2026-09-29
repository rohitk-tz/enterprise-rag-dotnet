using Contracts.Projects;
using MediatR;

namespace Application.Projects.Commands.UpdateProjectSettings;

public record UpdateProjectSettingsCommand(
    Guid ProjectId, string EmbeddingModel, string RagStrategy, string AgentType,
    int ChunksPerSearch, int FinalContextSize, decimal SimilarityThreshold,
    int NumberOfQueries, bool RerankingEnabled, string RerankingModel,
    decimal VectorWeight, decimal KeywordWeight) : IRequest<ProjectSettingsResponse>;
