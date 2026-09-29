using FluentValidation;

namespace Application.Files.Commands.GenerateFileUploadUrl;

public class GenerateFileUploadUrlCommandValidator : AbstractValidator<GenerateFileUploadUrlCommand>
{
    public GenerateFileUploadUrlCommandValidator()
    {
        RuleFor(x => x.Filename).NotEmpty();
        RuleFor(x => x.FileType).NotEmpty();
        RuleFor(x => x.FileSize).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FileSize).LessThanOrEqualTo(int.MaxValue);
    }
}
