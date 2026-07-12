// Application/Features/Cattle/Commands/CreateCattle/CreateCattleCommandValidator.cs
using FluentValidation;

namespace Application.Features.Cattle.Commands.CreateCattle;

public class CreateCattleCommandValidator : AbstractValidator<CreateCattleCommand>
{
    public CreateCattleCommandValidator()
    {
        RuleFor(x => x.TagNumber)
            .NotEmpty().WithMessage("Tag number is required")
            .MaximumLength(50);

        RuleFor(x => x.Breed)
            .NotEmpty().WithMessage("Breed is required")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .LessThan(DateTimeOffset.UtcNow).WithMessage("Birth date must be in the past");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender");
    }
}