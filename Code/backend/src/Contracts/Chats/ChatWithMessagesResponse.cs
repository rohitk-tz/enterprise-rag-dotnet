namespace Contracts.Chats;

public record ChatWithMessagesResponse(
    Guid Id, string Title, Guid ProjectId, DateTimeOffset CreatedAt, List<MessageResponse> Messages);
