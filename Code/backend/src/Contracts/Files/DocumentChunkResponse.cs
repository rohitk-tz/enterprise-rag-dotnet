namespace Contracts.Files;

public record DocumentChunkResponse(
    Guid Id, Guid DocumentId, string Content, int ChunkIndex, int? PageNumber,
    int CharCount, DateTimeOffset CreatedAt);
