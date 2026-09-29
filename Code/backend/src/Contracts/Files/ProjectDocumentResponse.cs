namespace Contracts.Files;

public record ProjectDocumentResponse(
    Guid Id, Guid ProjectId, string Filename, long FileSize, string FileType,
    string ProcessingStatus, string SourceType, string? SourceUrl, DateTimeOffset CreatedAt);
