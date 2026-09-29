using SharedKernel;

namespace Domain.Entities;

public class Project : Entity<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string ClerkId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Project(Guid id, string name, string? description, string clerkId, DateTimeOffset createdAt) : base(id)
    {
        Name = name;
        Description = description;
        ClerkId = clerkId;
        CreatedAt = createdAt;
    }
}
