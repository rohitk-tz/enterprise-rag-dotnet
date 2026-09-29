using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsByClerkIdAsync(string clerkId, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.ClerkId == clerkId, ct);

    public Task<User?> GetByClerkIdAsync(string clerkId, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.ClerkId == clerkId, ct);

    public async Task AddAsync(User user, CancellationToken ct) => await _context.Users.AddAsync(user, ct);
}
