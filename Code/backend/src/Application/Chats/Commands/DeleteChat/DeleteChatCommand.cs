using MediatR;

namespace Application.Chats.Commands.DeleteChat;

public record DeleteChatCommand(Guid ChatId) : IRequest;
