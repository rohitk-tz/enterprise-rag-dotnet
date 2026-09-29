using System.Text;
using Application.Common.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infrastructure.Ai;

public class SemanticKernelSummarizationService : ISummarizationService
{
    private readonly IChatCompletionService _chatService;

    public SemanticKernelSummarizationService(Kernel kernel)
    {
        _chatService = kernel.GetRequiredService<IChatCompletionService>("summarization");
    }

    public async Task<string> SummarizeAsync(
        string text, IReadOnlyList<string> tables, IReadOnlyList<string> images, CancellationToken ct)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("Create a searchable index for this document content.");
        prompt.AppendLine();
        prompt.AppendLine("CONTENT:");
        prompt.AppendLine(text);
        prompt.AppendLine();

        if (tables.Count > 0)
        {
            prompt.AppendLine("TABLES:");
            for (var i = 0; i < tables.Count; i++)
            {
                prompt.AppendLine($"Table {i + 1}:");
                prompt.AppendLine(tables[i]);
                prompt.AppendLine();
            }
        }

        prompt.AppendLine();
        prompt.Append(
            "Generate a structured search index (aim for 250-400 words):\n\n" +
            "QUESTIONS: List 5-7 key questions this content answers (use what/how/why/when/who variations)\n\n" +
            "KEYWORDS: Include:\n" +
            "- Specific data (numbers, dates, percentages, amounts)\n" +
            "- Core concepts and themes\n" +
            "- Technical terms and casual alternatives\n" +
            "- Industry terminology\n\n" +
            "VISUALS (if images present):\n" +
            "- Chart/graph types and what they show\n" +
            "- Trends and patterns visible\n" +
            "- Key insights from visualizations\n\n" +
            "DATA RELATIONSHIPS (if tables present):\n" +
            "- Column headers and their meaning\n" +
            "- Key metrics and relationships\n" +
            "- Notable values or patterns\n\n" +
            "Focus on terms users would actually search for. Be specific and comprehensive.\n\n" +
            "SEARCH INDEX:");

        var history = new ChatHistory();
        var messageItems = new ChatMessageContentItemCollection { new TextContent(prompt.ToString()) };
        foreach (var imageBase64 in images)
        {
            messageItems.Add(new ImageContent(Convert.FromBase64String(imageBase64), "image/jpeg"));
        }
        history.AddUserMessage(messageItems);

        var response = await _chatService.GetChatMessageContentAsync(history, cancellationToken: ct);
        return response.Content ?? string.Empty;
    }
}
