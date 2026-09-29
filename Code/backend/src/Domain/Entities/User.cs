using SharedKernel;

namespace Domain.Entities;

public class User : Entity<Guid>
{
    public string ClerkId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public User(Guid id, string clerkId, DateTimeOffset createdAt) : base(id)
    {
        ClerkId = clerkId;
        CreatedAt = createdAt;
    }
}
