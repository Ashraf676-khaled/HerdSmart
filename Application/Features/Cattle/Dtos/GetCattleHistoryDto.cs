using HerdSmart.Domain.Enums;

public record HealthLogDto(
    Ulid Id,
    string Diagnosis,
    string TreatmentPlan,
    string? VetNotes,
    DateTimeOffset CreatedAt);
public record CattleHistoryResponse(
    IEnumerable<HealthLogDto> HealthLogs,
    IEnumerable<MilkLogDto> MilkLogs,
    IEnumerable<VaccinationDto> Vaccinations);

public record MilkLogDto(
    Ulid Id,
    double AmountInLiters,
    MilkShift Shift,
    DateTimeOffset LoggedAt);

public record VaccinationDto(
    Ulid Id,
    string VaccineName,
    DateTimeOffset ScheduledDate,
    DateTimeOffset? AdministeredDate,
    VaccinationStatus Status);
