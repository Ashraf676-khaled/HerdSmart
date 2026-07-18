// Application/Features/Vaccinations/VaccinationScheduleResponse.cs
using HerdSmart.Domain.Enums;

namespace Application.Features.Vaccinations;

public record VaccinationScheduleResponse(
    Ulid Id,
    Ulid CattleId,
    string CattleTagNumber,
    Ulid VaccineId,
    string VaccineName,
    DateTimeOffset ScheduledDate,
    DateTimeOffset? AdministeredDate,
    VaccinationStatus Status);