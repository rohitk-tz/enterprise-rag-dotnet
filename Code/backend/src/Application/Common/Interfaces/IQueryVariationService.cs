namespace Application.Common.Interfaces;

public interface IQueryVariationService
{
    Task<List<string>> GenerateVariationsAsync(string originalQuery, int numberOfQueries, CancellationToken ct);
}
