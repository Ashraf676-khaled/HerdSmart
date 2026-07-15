// Application/Features/Vaccines/Commands/UpdateVaccine/UpdateVaccineCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccines.Commands.UpdateVaccine;

public class UpdateVaccineCommandValidator : AbstractValidator<UpdateVaccineCommand>
{
    public UpdateVaccineCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TargetAgeInMonths)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dosage)
            .GreaterThan(0);

        RuleFor(x => x.IntervalInDays)
            .GreaterThan(0)
            .When(x => x.IntervalInDays.HasValue);
    }
}