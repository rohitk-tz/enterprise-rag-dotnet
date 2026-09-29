using Contracts.Chats;
using MediatR;

namespace Application.Chats.Queries.GetChatWithMessages;

public record GetChatWithMessagesQuery(Guid ChatId) : IRequest<ChatWithMessagesResponse>;
