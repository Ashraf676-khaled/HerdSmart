// Entities/MilkProductionLog.cs
using HerdSmart.Domain.Enums;

namespace HerdSmart.Domain.Entities;

public class MilkProductionLog : BaseEntity
{
    public Ulid TenantId { get; set; }
    public Ulid CattleId { get; set; }
    public double AmountInLiters { get; set; }
    public MilkShift Shift { get; set; }
    public DateTimeOffset LoggedAt { get; set; }

    public Cattle Cattle { get; set; } = null!;
}