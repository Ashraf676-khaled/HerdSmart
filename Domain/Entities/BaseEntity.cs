// Entities/BaseEntity.cs
namespace HerdSmart.Domain.Entities;

public abstract class BaseEntity
{
    public Ulid Id { get; set; } = Ulid.NewUlid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}