using Application.Features.Auth.Commands.Register;
using FluentValidation;

namespace HerdSmart.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FarmName)
                .NotEmpty().WithMessage("Farm name is required")
                .MaximumLength(50).WithMessage("Farm name must not exceed 50 characters."); ;

            RuleFor(x => x.OwnerName)
                .NotEmpty().WithMessage("Owner name is required")
                .MaximumLength(50).WithMessage("Owner name must not exceed 50 characters."); ;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");
                
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(32).WithMessage("Password must not exceed 32 characters.")
                //regex
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\^$*.\[\]{}()?""!@#%&/\ loyalty, _=+<>:-]")
                .WithMessage("Password must contain at least one special character (e.g. @, #, $, %, etc.)."); ;
        }
    }
}