using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByClerkIdAsync(string clerkId, CancellationToken ct);
    Task<User?> GetByClerkIdAsync(string clerkId, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
