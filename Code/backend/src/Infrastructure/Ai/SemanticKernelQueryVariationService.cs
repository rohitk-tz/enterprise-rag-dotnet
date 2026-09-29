using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Infrastructure.Ai;

public class SemanticKernelQueryVariationService : IQueryVariationService
{
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<SemanticKernelQueryVariationService> _logger;

    public SemanticKernelQueryVariationService(Kernel kernel, ILogger<SemanticKernelQueryVariationService> logger)
    {
        _chatService = kernel.GetRequiredService<IChatCompletionService>("chat");
        _logger = logger;
    }

    public async Task<List<string>> GenerateVariationsAsync(string originalQuery, int numberOfQueries, CancellationToken ct)
    {
        var variationsNeeded = numberOfQueries - 1;

        try
        {
            var history = new ChatHistory();
            history.AddSystemMessage(
                $"Generate {variationsNeeded} alternative ways to phrase this question for document search. " +
                "Use different keywords and synonyms while maintaining the same intent. " +
                $"Return exactly {variationsNeeded} variations.");
            history.AddUserMessage($"Original query: {originalQuery}");

            var settings = new OpenAIPromptExecutionSettings { ResponseFormat = typeof(QueryVariationsDto) };
            var response = await _chatService.GetChatMessageContentAsync(history, settings, cancellationToken: ct);

            var parsed = JsonSerializer.Deserialize<QueryVariationsDto>(response.Content!)
                ?? throw new InvalidOperationException("Query variation generation returned an empty response.");

            return new List<string> { originalQuery }.Concat(parsed.Queries.Take(variationsNeeded)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Query variation generation failed; falling back to the original query only");
            return new List<string> { originalQuery };
        }
    }

    private record QueryVariationsDto(List<string> Queries);
}
