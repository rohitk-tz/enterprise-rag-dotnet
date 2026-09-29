namespace Application.Common.Interfaces;

public interface IChatAnswerService
{
    Task<string> AnswerAsync(string userQuery, List<string> texts, List<string> images, List<string> tables, CancellationToken ct);
}
