using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Chat>> GetByProjectIdAsync(Guid projectId, CancellationToken ct) =>
        await _context.Chats
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task<Chat?> GetByIdForOwnerAsync(Guid id, string clerkId, CancellationToken ct) =>
        await _context.Chats.FirstOrDefaultAsync(c => c.Id == id && c.ClerkId == clerkId, ct);

    public async Task<List<Message>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken ct) =>
        await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Chat chat, CancellationToken ct) => await _context.Chats.AddAsync(chat, ct);

    public void Remove(Chat chat) => _context.Chats.Remove(chat);
}
