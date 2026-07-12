using FluentValidation;

namespace Application.Features.User.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Fullname is required")
            .MaximumLength(100).WithMessage("Maximum length is 100");

        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(32).WithMessage("Password must not exceed 32 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\^$*.\[\]{}()?""!@#%&/\ loyalty, _=+<>:-]")
                .WithMessage("Password must contain at least one special character (e.g. @, #, $, %, etc.).");
        });
    }
}