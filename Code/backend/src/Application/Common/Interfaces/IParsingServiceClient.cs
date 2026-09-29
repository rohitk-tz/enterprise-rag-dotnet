using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IParsingServiceClient
{
    Task<PartitionAndChunkResult> PartitionAndChunkAsync(
        Stream fileContent, string fileType, string sourceType, CancellationToken ct);
}
