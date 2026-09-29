namespace Contracts.Chats;

public record MessageResponse(
    Guid Id, string Content, string Role, Guid ChatId, DateTimeOffset CreatedAt, List<CitationDto>? Citations);
