// Application/Features/Vaccinations/Commands/CreateVaccinationSchedule/CreateVaccinationScheduleCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;

public class CreateVaccinationScheduleCommandValidator
    : AbstractValidator<CreateVaccinationScheduleCommand>
{
    public CreateVaccinationScheduleCommandValidator()
    {
        RuleFor(x => x.CattleId).NotEmpty();
        RuleFor(x => x.VaccineId).NotEmpty();
        RuleFor(x => x.ScheduledDate).NotEmpty();
    }
}