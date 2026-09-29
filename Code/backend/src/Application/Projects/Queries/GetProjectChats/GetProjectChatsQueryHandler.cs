using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Chats;
using MediatR;

namespace Application.Projects.Queries.GetProjectChats;

public class GetProjectChatsQueryHandler : IRequestHandler<GetProjectChatsQuery, List<ChatResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserAccessor _currentUser;

    public GetProjectChatsQueryHandler(
        IProjectRepository projectRepository, IChatRepository chatRepository, ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _chatRepository = chatRepository;
        _currentUser = currentUser;
    }

    public async Task<List<ChatResponse>> Handle(GetProjectChatsQuery request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var chats = await _chatRepository.GetByProjectIdAsync(request.ProjectId, ct);

        return chats
            .Select(c => new ChatResponse(c.Id, c.Title, c.ProjectId, c.CreatedAt))
            .ToList();
    }
}
