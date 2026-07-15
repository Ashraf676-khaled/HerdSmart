// Application/Features/Vaccines/Commands/CreateVaccine/CreateVaccineCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccines.Commands.CreateVaccine;

public class CreateVaccineCommandValidator : AbstractValidator<CreateVaccineCommand>
{
    public CreateVaccineCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TargetAgeInMonths)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dosage)
            .GreaterThan(0);

        RuleFor(x => x.IntervalInDays)
            .GreaterThan(0)
            .When(x => x.IntervalInDays.HasValue)
            .WithMessage("Interval must be greater than 0 when provided");
    }
}