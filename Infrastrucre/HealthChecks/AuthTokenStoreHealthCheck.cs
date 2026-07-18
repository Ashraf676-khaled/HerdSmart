// Infrastructure/HealthChecks/AuthTokenStoreHealthCheck.cs
using Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class AuthTokenStoreHealthCheck(IRefreshTokenService refreshTokenService) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var probeUserId = Guid.Empty;
        var probeValue = $"probe-{DateTimeOffset.UtcNow.Ticks}";

        try
        {
            // تنفيذ عملية اختبار كاملة (Write -> Read -> Delete)
            await refreshTokenService.SaveRefreshTokenAsync(probeUserId, probeValue);
            var readBack = await refreshTokenService.GetRefreshTokenAsync(probeUserId);
            await refreshTokenService.RevokeRefreshTokenAsync(probeUserId);

            if (readBack == probeValue)
            {
                return HealthCheckResult.Healthy("Refresh token store (Redis) read and write operations are functioning correctly.");
            }

            return HealthCheckResult.Unhealthy("Refresh token store returned an unexpected value during data verification probe.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to communicate with the Refresh token store.", ex);
        }
    }
}