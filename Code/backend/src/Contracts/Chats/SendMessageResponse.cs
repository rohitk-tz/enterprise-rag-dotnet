namespace Contracts.Chats;

public record SendMessageResponse(MessageResponse UserMessage, MessageResponse AiMessage);
