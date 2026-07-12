// HerdSmart.Infrastructure/Services/TenantProvider.cs
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HerdSmart.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Ulid GetTenantId()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim))
                throw new UnauthorizedAccessException("Tenant not found in token. Please login again.");

            if (!Ulid.TryParse(tenantIdClaim, out var tenantId))
                throw new UnauthorizedAccessException("Invalid Tenant ID format in token.");

            return tenantId;
        }

        public Guid GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token. Please login again.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid User ID format in token.");

            return userId;
        }

        public string GetUserRole()
        {
            var role = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
                throw new UnauthorizedAccessException("Role not found in token. Please login again.");

            return role;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}