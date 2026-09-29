using SharedKernel;

namespace Domain.Entities;

public class Message : Entity<Guid>
{
    public string Content { get; private set; }
    public string Role { get; private set; }
    public Guid ChatId { get; private set; }
    public string ClerkId { get; private set; }
    public string CitationsJson { get; private set; }
    public string? TraceId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Message(
        Guid id, string content, string role, Guid chatId, string clerkId,
        string citationsJson, string? traceId, DateTimeOffset createdAt) : base(id)
    {
        Content = content;
        Role = role;
        ChatId = chatId;
        ClerkId = clerkId;
        CitationsJson = citationsJson;
        TraceId = traceId;
        CreatedAt = createdAt;
    }
}
