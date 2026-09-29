using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IChatRepository
{
    Task<List<Chat>> GetByProjectIdAsync(Guid projectId, CancellationToken ct);
    Task<Chat?> GetByIdForOwnerAsync(Guid id, string clerkId, CancellationToken ct);
    Task<List<Message>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken ct);
    Task AddAsync(Chat chat, CancellationToken ct);
    void Remove(Chat chat);
}
