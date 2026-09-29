using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Chats;
using Domain.Entities;
using MediatR;

namespace Application.Chats.Commands.CreateChat;

public class CreateChatCommandHandler : IRequestHandler<CreateChatCommand, ChatResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateChatCommandHandler(
        IProjectRepository projectRepository, IChatRepository chatRepository, IUnitOfWork unitOfWork, ICurrentUserAccessor currentUser)
    {
        _projectRepository = projectRepository;
        _chatRepository = chatRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ChatResponse> Handle(CreateChatCommand request, CancellationToken ct)
    {
        _ = await _projectRepository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        var chat = new Chat(Guid.NewGuid(), request.Title, request.ProjectId, _currentUser.ClerkId, DateTimeOffset.UtcNow);
        await _chatRepository.AddAsync(chat, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ChatResponse(chat.Id, chat.Title, chat.ProjectId, chat.CreatedAt);
    }
}
