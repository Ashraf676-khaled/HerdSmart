// Entities/Tenant.cs
using HerdSmart.Domain.Enums;

namespace HerdSmart.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SubscriptionPlan Plan { get; set; }

    public ICollection<Cattle> Cattle { get; set; } = new List<Cattle>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}