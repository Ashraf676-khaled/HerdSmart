// Application/Features/Auth/Commands/CreateUser/CreateUserCommandValidator.cs
using FluentValidation;
using HerdSmart.Domain.Enums;

namespace Application.Features.User.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is wrong");

        RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(32).WithMessage("Password must not exceed 32 characters.")
                //regex
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\^$*.\[\]{}()?""!@#%&/\ loyalty, _=+<>:-]")
                .WithMessage("Password must contain at least one special character (e.g. @, #, $, %, etc.)."); 
    

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.Vet || r == UserRole.Worker)
            .WithMessage("Role must be Vet or Worker");
    }
}