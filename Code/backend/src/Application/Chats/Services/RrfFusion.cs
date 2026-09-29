using Domain.Entities;

namespace Application.Chats.Services;

public static class RrfFusion
{
    private const int DefaultK = 60;

    /// <summary>
    /// Reciprocal Rank Fusion: combines multiple ranked result sets into one, scoring each
    /// chunk by weight * (1 / (k + rank + 1)) summed across every set it appears in, then
    /// sorting descending by total score. Equal weighting is used when none are supplied.
    /// </summary>
    public static List<DocumentChunk> Fuse(
        IReadOnlyList<List<DocumentChunk>> resultSets, IReadOnlyList<double>? weights = null, int k = DefaultK)
    {
        if (resultSets.Count == 0 || resultSets.All(r => r.Count == 0))
        {
            return new List<DocumentChunk>();
        }

        var effectiveWeights = weights ?? Enumerable.Repeat(1.0 / resultSets.Count, resultSets.Count).ToList();

        var scores = new Dictionary<Guid, double>();
        var chunksById = new Dictionary<Guid, DocumentChunk>();

        for (var setIndex = 0; setIndex < resultSets.Count; setIndex++)
        {
            var weight = effectiveWeights[setIndex];
            var results = resultSets[setIndex];

            for (var rank = 0; rank < results.Count; rank++)
            {
                var chunk = results[rank];
                var rrfScore = weight * (1.0 / (k + rank + 1));

                scores[chunk.Id] = scores.TryGetValue(chunk.Id, out var existing) ? existing + rrfScore : rrfScore;
                chunksById.TryAdd(chunk.Id, chunk);
            }
        }

        return scores
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => chunksById[kvp.Key])
            .ToList();
    }
}
