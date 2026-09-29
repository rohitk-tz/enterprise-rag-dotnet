namespace Application.Common.Interfaces;

public interface ISummarizationService
{
    Task<string> SummarizeAsync(string text, IReadOnlyList<string> tables, IReadOnlyList<string> images, CancellationToken ct);
}
