// Application/Features/Vaccines/Commands/DeleteVaccine/DeleteVaccineCommand.cs
using MediatR;

namespace Application.Features.Vaccines.Commands.DeleteVaccine;

public record DeleteVaccineCommand(Ulid Id) : IRequest;