// Application/Features/Vaccines/Commands/CreateVaccine/CreateVaccineCommand.cs
using MediatR;

namespace Application.Features.Vaccines.Commands.CreateVaccine;

public record CreateVaccineCommand(
    string Name,
    int TargetAgeInMonths,
    double Dosage,
    int? IntervalInDays) : IRequest<VaccineResponse>;