// Entities/HealthLog.cs
namespace HerdSmart.Domain.Entities;

public class HealthLog : BaseEntity
{
    public Ulid TenantId { get; set; }
    public Ulid CattleId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string? VetNotes { get; set; }

    public Cattle Cattle { get; set; } = null!;
}