namespace Application.Common.Models;

public record PartitionAndChunkResult(IReadOnlyList<RawChunk> Chunks, int TotalChunks);
