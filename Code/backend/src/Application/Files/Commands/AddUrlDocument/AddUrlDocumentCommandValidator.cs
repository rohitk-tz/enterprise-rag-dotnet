using FluentValidation;

namespace Application.Files.Commands.AddUrlDocument;

public class AddUrlDocumentCommandValidator : AbstractValidator<AddUrlDocumentCommand>
{
    public AddUrlDocumentCommandValidator()
    {
        RuleFor(x => x.Url).NotEmpty().Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URL.");
    }
}
