using FluentValidation;

namespace Application.Files.Commands.ConfirmFileUpload;

public class ConfirmFileUploadCommandValidator : AbstractValidator<ConfirmFileUploadCommand>
{
    public ConfirmFileUploadCommandValidator()
    {
        RuleFor(x => x.S3Key).NotEmpty();
    }
}
