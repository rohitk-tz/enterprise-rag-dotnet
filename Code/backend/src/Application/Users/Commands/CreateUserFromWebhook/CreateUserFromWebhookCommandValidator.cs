using FluentValidation;

namespace Application.Users.Commands.CreateUserFromWebhook;

public class CreateUserFromWebhookCommandValidator : AbstractValidator<CreateUserFromWebhookCommand>
{
    public CreateUserFromWebhookCommandValidator()
    {
        RuleFor(x => x.EventType).Equal("user.created")
            .WithMessage("Invalid webhook data: unsupported event type.");
        RuleFor(x => x.ClerkId).NotEmpty()
            .WithMessage("Invalid webhook data: 'id' not found.");
    }
}
