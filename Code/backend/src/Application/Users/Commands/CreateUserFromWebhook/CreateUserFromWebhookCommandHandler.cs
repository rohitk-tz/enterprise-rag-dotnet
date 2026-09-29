using Application.Common.Interfaces;
using Contracts.Users;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.CreateUserFromWebhook;

public class CreateUserFromWebhookCommandHandler : IRequestHandler<CreateUserFromWebhookCommand, UserResponse>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserFromWebhookCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> Handle(CreateUserFromWebhookCommand request, CancellationToken ct)
    {
        var clerkId = request.ClerkId!;

        // Clerk retries webhook deliveries as standard practice, so the same "user.created"
        // event for the same clerk_id can arrive more than once. Treat a redelivery as a
        // no-op success rather than attempting a duplicate insert (which would violate the
        // clerk_id unique constraint and surface as a 500).
        if (await _repository.ExistsByClerkIdAsync(clerkId, ct))
        {
            var existingUser = await _repository.GetByClerkIdAsync(clerkId, ct);
            return new UserResponse(existingUser!.Id, existingUser.ClerkId);
        }

        var user = new User(Guid.NewGuid(), clerkId, DateTimeOffset.UtcNow);

        await _repository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserResponse(user.Id, user.ClerkId);
    }
}
