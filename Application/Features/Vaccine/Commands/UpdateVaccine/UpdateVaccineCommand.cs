// Application/Features/Vaccines/Commands/UpdateVaccine/UpdateVaccineCommand.cs
using MediatR;

namespace Application.Features.Vaccines.Commands.UpdateVaccine;

public record UpdateVaccineCommand(
    Ulid Id,
    string Name,
    int TargetAgeInMonths,
    double Dosage,
    int? IntervalInDays) : IRequest<VaccineResponse>;