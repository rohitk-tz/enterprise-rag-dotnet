using FluentValidation;

namespace Application.Chats.Commands.CreateChat;

public class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}
