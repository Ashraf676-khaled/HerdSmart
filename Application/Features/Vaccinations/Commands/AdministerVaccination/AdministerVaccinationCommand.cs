// Application/Features/Vaccinations/Commands/AdministerVaccination/AdministerVaccinationCommand.cs
using MediatR;

namespace Application.Features.Vaccinations.Commands.AdministerVaccination;

public record AdministerVaccinationCommand(
    Ulid Id,
    DateTimeOffset? AdministeredDate) : IRequest<VaccinationScheduleResponse>;