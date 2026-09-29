using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Chats.Commands.DeleteChat;

public class DeleteChatCommandHandler : IRequestHandler<DeleteChatCommand>
{
    private readonly IChatRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public DeleteChatCommandHandler(IChatRepository repository, IUnitOfWork unitOfWork, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteChatCommand request, CancellationToken ct)
    {
        // By-ID only, not scoped by project - matches server/routes/chats.py:delete_chat exactly
        // (see docs/migration/03-API-Inventory.md's note on this endpoint).
        var chat = await _repository.GetByIdForOwnerAsync(request.ChatId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Chat not found.");

        _repository.Remove(chat);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
