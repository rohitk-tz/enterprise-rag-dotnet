namespace Contracts.Chats;

public record ChatResponse(Guid Id, string Title, Guid ProjectId, DateTimeOffset CreatedAt);
