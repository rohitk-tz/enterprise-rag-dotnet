using Contracts.Chats;
using MediatR;

namespace Application.Projects.Queries.GetProjectChats;

public record GetProjectChatsQuery(Guid ProjectId) : IRequest<List<ChatResponse>>;
