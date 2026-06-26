// Entities/Cattle.cs
using HerdSmart.Domain.Enums;

namespace HerdSmart.Domain.Entities;

public class Cattle : BaseEntity
{
    public Ulid TenantId { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTimeOffset BirthDate { get; set; }
    public CattleStatus Status { get; set; } = CattleStatus.Active;
    public string? FatherTagNumber { get; set; }
    public string? MotherTagNumber { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<HealthLog> HealthLogs { get; set; } = new List<HealthLog>();
    public ICollection<VaccinationSchedule> Vaccinations { get; set; } = new List<VaccinationSchedule>();
    public ICollection<MilkProductionLog> MilkLogs { get; set; } = new List<MilkProductionLog>();
    public ICollection<TelemetryAlert> Alerts { get; set; } = new List<TelemetryAlert>();
}