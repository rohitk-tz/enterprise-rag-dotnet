namespace Contracts.Projects;

public record ProjectSettingsResponse(
    Guid Id,
    Guid ProjectId,
    string EmbeddingModel,
    string RagStrategy,
    string AgentType,
    int ChunksPerSearch,
    int FinalContextSize,
    decimal SimilarityThreshold,
    int NumberOfQueries,
    bool RerankingEnabled,
    string RerankingModel,
    decimal VectorWeight,
    decimal KeywordWeight);
