using SharedKernel;

namespace Domain.Entities;

public class Chat : Entity<Guid>
{
    public string Title { get; private set; }
    public Guid ProjectId { get; private set; }
    public string ClerkId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Chat(Guid id, string title, Guid projectId, string clerkId, DateTimeOffset createdAt) : base(id)
    {
        Title = title;
        ProjectId = projectId;
        ClerkId = clerkId;
        CreatedAt = createdAt;
    }
}
