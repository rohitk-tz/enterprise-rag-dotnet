using SharedKernel;

namespace Domain.Entities;

public class ProjectSettings : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string EmbeddingModel { get; private set; }
    public string RagStrategy { get; private set; }
    public string AgentType { get; private set; }
    public int ChunksPerSearch { get; private set; }
    public int FinalContextSize { get; private set; }
    public decimal SimilarityThreshold { get; private set; }
    public int NumberOfQueries { get; private set; }
    public bool RerankingEnabled { get; private set; }
    public string RerankingModel { get; private set; }
    public decimal VectorWeight { get; private set; }
    public decimal KeywordWeight { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ProjectSettings(
        Guid id, Guid projectId, string embeddingModel, string ragStrategy, string agentType,
        int chunksPerSearch, int finalContextSize, decimal similarityThreshold,
        int numberOfQueries, bool rerankingEnabled, string rerankingModel,
        decimal vectorWeight, decimal keywordWeight, DateTimeOffset createdAt) : base(id)
    {
        ProjectId = projectId;
        EmbeddingModel = embeddingModel;
        RagStrategy = ragStrategy;
        AgentType = agentType;
        ChunksPerSearch = chunksPerSearch;
        FinalContextSize = finalContextSize;
        SimilarityThreshold = similarityThreshold;
        NumberOfQueries = numberOfQueries;
        RerankingEnabled = rerankingEnabled;
        RerankingModel = rerankingModel;
        VectorWeight = vectorWeight;
        KeywordWeight = keywordWeight;
        CreatedAt = createdAt;
    }

    public void Update(
        string embeddingModel, string ragStrategy, string agentType, int chunksPerSearch,
        int finalContextSize, decimal similarityThreshold, int numberOfQueries,
        bool rerankingEnabled, string rerankingModel, decimal vectorWeight, decimal keywordWeight)
    {
        EmbeddingModel = embeddingModel;
        RagStrategy = ragStrategy;
        AgentType = agentType;
        ChunksPerSearch = chunksPerSearch;
        FinalContextSize = finalContextSize;
        SimilarityThreshold = similarityThreshold;
        NumberOfQueries = numberOfQueries;
        RerankingEnabled = rerankingEnabled;
        RerankingModel = rerankingModel;
        VectorWeight = vectorWeight;
        KeywordWeight = keywordWeight;
    }
}
