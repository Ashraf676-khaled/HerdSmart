// Application/Features/Vaccinations/Commands/CancelVaccination/CancelVaccinationCommand.cs
using MediatR;

namespace Application.Features.Vaccinations.Commands.CancelVaccination;

public record CancelVaccinationCommand(Ulid Id) : IRequest;