using FluentValidation;

namespace Application.Projects.Commands.UpdateProjectSettings;

public class UpdateProjectSettingsCommandValidator : AbstractValidator<UpdateProjectSettingsCommand>
{
    private static readonly string[] AllowedRagStrategies =
        ["basic", "hybrid", "multi-query-vector", "multi-query-hybrid"];

    public UpdateProjectSettingsCommandValidator()
    {
        RuleFor(x => x.EmbeddingModel).NotEmpty();
        RuleFor(x => x.RagStrategy)
            .Must(AllowedRagStrategies.Contains)
            .WithMessage($"rag_strategy must be one of: {string.Join(", ", AllowedRagStrategies)}");
        RuleFor(x => x.AgentType).NotEmpty();
        RuleFor(x => x.RerankingModel).NotEmpty();
        RuleFor(x => x.ChunksPerSearch).GreaterThan(0);
        RuleFor(x => x.FinalContextSize).GreaterThan(0);
        RuleFor(x => x.SimilarityThreshold).InclusiveBetween(0m, 1m);
        RuleFor(x => x.NumberOfQueries).GreaterThan(0);
        RuleFor(x => x.VectorWeight).InclusiveBetween(0m, 1m);
        RuleFor(x => x.KeywordWeight).InclusiveBetween(0m, 1m);
    }
}
