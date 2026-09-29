using FluentValidation;

namespace Application.Chats.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}
