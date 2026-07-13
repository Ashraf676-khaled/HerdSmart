// Application/Features/HealthLogs/Dtos/HealthLogResponse.cs
public record HealthLogResponse(
    Ulid Id,
    Ulid CattleId,
    string CattleTagNumber,
    string Diagnosis,
    string TreatmentPlan,
    string? VetNotes,
    bool IsContagious,
    DateTimeOffset CreatedAt);