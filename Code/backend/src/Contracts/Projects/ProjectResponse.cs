namespace Contracts.Projects;

public record ProjectResponse(Guid Id, string Name, string? Description, DateTimeOffset CreatedAt);
