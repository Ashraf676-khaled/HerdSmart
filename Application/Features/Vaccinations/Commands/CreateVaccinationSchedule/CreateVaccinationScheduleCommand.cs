// Application/Features/Vaccinations/Commands/CreateVaccinationSchedule/CreateVaccinationScheduleCommand.cs
using MediatR;

namespace Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;

public record CreateVaccinationScheduleCommand(
    Ulid CattleId,
    Ulid VaccineId,
    DateTimeOffset ScheduledDate) : IRequest<VaccinationScheduleResponse>;