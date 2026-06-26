// Entities/VaccinationSchedule.cs
using HerdSmart.Domain.Enums;

namespace HerdSmart.Domain.Entities;

public class VaccinationSchedule : BaseEntity
{
    public Ulid TenantId { get; set; }
    public Ulid CattleId { get; set; }
    public Ulid VaccineId { get; set; }
    public DateTimeOffset ScheduledDate { get; set; }
    public DateTimeOffset? AdministeredDate { get; set; }
    public VaccinationStatus Status { get; set; } = VaccinationStatus.Pending;

    public Cattle Cattle { get; set; } = null!;
    public Vaccine Vaccine { get; set; } = null!;
}