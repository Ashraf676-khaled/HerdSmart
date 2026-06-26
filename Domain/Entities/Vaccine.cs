// Entities/Vaccine.cs
namespace HerdSmart.Domain.Entities;

public class Vaccine
{
    public Ulid Id { get; set; } = Ulid.NewUlid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Ulid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TargetAgeInMonths { get; set; }
    public double Dosage { get; set; }
    public int? IntervalInDays { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<VaccinationSchedule> Schedules { get; set; } = new List<VaccinationSchedule>();
}