using Contracts.Chats;
using MediatR;

namespace Application.Chats.Commands.SendMessage;

public record SendMessageCommand(Guid ProjectId, Guid ChatId, string Content) : IRequest<SendMessageResponse>;
