// Entities/AppUser.cs
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace HerdSmart.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{

    public Ulid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }

    public Tenant Tenant { get; set; } = null!;
}