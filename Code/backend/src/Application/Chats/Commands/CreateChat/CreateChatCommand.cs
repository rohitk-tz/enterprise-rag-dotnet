using Contracts.Chats;
using MediatR;

namespace Application.Chats.Commands.CreateChat;

public record CreateChatCommand(Guid ProjectId, string Title) : IRequest<ChatResponse>;
