// Application/Features/Vaccinations/Commands/RescheduleVaccination/RescheduleVaccinationCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccinations.Commands.RescheduleVaccination;

public class RescheduleVaccinationCommandValidator
    : AbstractValidator<RescheduleVaccinationCommand>
{
    public RescheduleVaccinationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewScheduledDate).NotEmpty();
    }
}