using Contracts.Users;
using MediatR;

namespace Application.Users.Commands.CreateUserFromWebhook;

public record CreateUserFromWebhookCommand(string? EventType, string? ClerkId) : IRequest<UserResponse>;
