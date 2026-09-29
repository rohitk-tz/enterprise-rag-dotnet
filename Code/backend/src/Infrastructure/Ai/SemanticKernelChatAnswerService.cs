using System.Text;
using Application.Common.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infrastructure.Ai;

public class SemanticKernelChatAnswerService : IChatAnswerService
{
    private readonly IChatCompletionService _chatService;

    public SemanticKernelChatAnswerService(Kernel kernel)
    {
        _chatService = kernel.GetRequiredService<IChatCompletionService>("chat");
    }

    public async Task<string> AnswerAsync(
        string userQuery, List<string> texts, List<string> images, List<string> tables, CancellationToken ct)
    {
        var systemPrompt = BuildSystemPrompt(texts, tables, images);

        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);

        if (images.Count > 0)
        {
            var items = new ChatMessageContentItemCollection { new TextContent(userQuery) };
            foreach (var imageBase64 in images)
            {
                var cleaned = imageBase64.Contains(',') ? imageBase64.Split(',', 2)[1] : imageBase64;
                items.Add(new ImageContent(Convert.FromBase64String(cleaned), "image/jpeg"));
            }
            history.AddUserMessage(items);
        }
        else
        {
            history.AddUserMessage(userQuery);
        }

        var response = await _chatService.GetChatMessageContentAsync(history, cancellationToken: ct);
        return response.Content ?? string.Empty;
    }

    private static string BuildSystemPrompt(List<string> texts, List<string> tables, List<string> images)
    {
        var parts = new StringBuilder();

        parts.AppendLine(
            "You are a helpful AI assistant that answers questions based solely on the provided context. " +
            "Your task is to provide accurate, detailed answers using ONLY the information available in the context below.\n\n" +
            "IMPORTANT RULES:\n" +
            "- Only answer based on the provided context (texts, tables, and images)\n" +
            "- If the answer cannot be found in the context, respond with: 'I don't have enough information in the provided context to answer that question.'\n" +
            "- Do not use external knowledge or make assumptions beyond what's explicitly stated\n" +
            "- When referencing information, be specific and cite relevant parts of the context\n" +
            "- Synthesize information from texts, tables, and images to provide comprehensive answers\n\n");

        if (texts.Count > 0)
        {
            parts.AppendLine(new string('=', 80));
            parts.AppendLine("CONTEXT DOCUMENTS");
            parts.AppendLine(new string('=', 80) + "\n");

            for (var i = 0; i < texts.Count; i++)
            {
                parts.AppendLine($"--- Document Chunk {i + 1} ---");
                parts.AppendLine(texts[i].Trim());
                parts.AppendLine();
            }
        }

        if (tables.Count > 0)
        {
            parts.AppendLine("\n" + new string('=', 80));
            parts.AppendLine("RELATED TABLES");
            parts.AppendLine(new string('=', 80));
            parts.AppendLine(
                "The following tables contain structured data that may be relevant to your answer. " +
                "Analyze the table contents carefully.\n");

            for (var i = 0; i < tables.Count; i++)
            {
                parts.AppendLine($"--- Table {i + 1} ---");
                parts.AppendLine(tables[i]);
                parts.AppendLine();
            }
        }

        if (images.Count > 0)
        {
            parts.AppendLine("\n" + new string('=', 80));
            parts.AppendLine("RELATED IMAGES");
            parts.AppendLine(new string('=', 80));
            parts.AppendLine(
                $"{images.Count} image(s) will be provided alongside the user's question. " +
                "These images may contain diagrams, charts, figures, formulas, or other visual information. " +
                "Carefully analyze the visual content when formulating your response. " +
                "The images are part of the retrieved context and should be used to answer the question.\n");
        }

        parts.AppendLine(new string('=', 80));
        parts.AppendLine(
            "Based on all the context provided above (documents, tables, and images), " +
            "please answer the user's question accurately and comprehensively.");
        parts.Append(new string('=', 80));

        return parts.ToString();
    }
}
