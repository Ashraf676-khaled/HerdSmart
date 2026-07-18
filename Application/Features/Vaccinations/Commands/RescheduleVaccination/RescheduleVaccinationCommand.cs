// Application/Features/Vaccinations/Commands/RescheduleVaccination/RescheduleVaccinationCommand.cs
using MediatR;

namespace Application.Features.Vaccinations.Commands.RescheduleVaccination;

public record RescheduleVaccinationCommand(
    Ulid Id,
    DateTimeOffset NewScheduledDate) : IRequest<VaccinationScheduleResponse>;