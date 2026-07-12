using Application.Features.Cattle.Commands.UpdateCattle;
using FluentValidation;

public class UpdateCattleCommandValidator : AbstractValidator<UpdateCattleCommand>
{
    public UpdateCattleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cattle ID is required");

        RuleFor(x => x.TagNumber)
            .NotEmpty().WithMessage("Tag number is required")
            .MaximumLength(50);

        RuleFor(x => x.Breed)
            .NotEmpty().WithMessage("Breed is required")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .LessThan(DateTimeOffset.UtcNow).WithMessage("Birth date must be in the past");
    }
}