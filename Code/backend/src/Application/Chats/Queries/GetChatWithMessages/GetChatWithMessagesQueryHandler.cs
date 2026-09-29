using System.Text.Json;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Chats;
using MediatR;

namespace Application.Chats.Queries.GetChatWithMessages;

public class GetChatWithMessagesQueryHandler : IRequestHandler<GetChatWithMessagesQuery, ChatWithMessagesResponse>
{
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserAccessor _currentUser;

    private static readonly JsonSerializerOptions CitationDeserializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public GetChatWithMessagesQueryHandler(IChatRepository chatRepository, ICurrentUserAccessor currentUser)
    {
        _chatRepository = chatRepository;
        _currentUser = currentUser;
    }

    public async Task<ChatWithMessagesResponse> Handle(GetChatWithMessagesQuery request, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdForOwnerAsync(request.ChatId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Chat not found.");

        var messages = await _chatRepository.GetMessagesByChatIdAsync(request.ChatId, ct);

        return new ChatWithMessagesResponse(
            chat.Id, chat.Title, chat.ProjectId, chat.CreatedAt,
            messages.Select(m =>
            {
                var citations = string.IsNullOrEmpty(m.CitationsJson)
                    ? new List<CitationDto>()
                    : JsonSerializer.Deserialize<List<CitationDto>>(m.CitationsJson, CitationDeserializeOptions) ?? new List<CitationDto>();

                return new MessageResponse(m.Id, m.Content, m.Role, m.ChatId, m.CreatedAt, citations);
            }).ToList());
    }
}
