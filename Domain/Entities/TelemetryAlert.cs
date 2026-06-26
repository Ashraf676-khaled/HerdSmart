// Entities/TelemetryAlert.cs
using HerdSmart.Domain.Enums;

namespace HerdSmart.Domain.Entities;

public class TelemetryAlert : BaseEntity
{
    public Ulid TenantId { get; set; }
    public Ulid CattleId { get; set; }
    public SensorType SensorType { get; set; }
    public double Value { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public bool IsResolved { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public Cattle Cattle { get; set; } = null!;
}