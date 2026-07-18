// Application/Features/Vaccines/VaccineResponse.cs
namespace Application.Features.Vaccines;

public record VaccineResponse(
    Ulid Id,
    string Name,
    int TargetAgeInMonths,
    double Dosage,
    int? IntervalInDays);