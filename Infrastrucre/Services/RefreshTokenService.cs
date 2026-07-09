// Infrastructure/Services/RefreshTokenService.cs
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IDistributedCache _cache;

    public RefreshTokenService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string token)
    {
        await _cache.SetStringAsync(
            $"refresh:{userId}",
            token,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            }
            );
    }

    public async Task<string?> GetRefreshTokenAsync(Guid userId)
    {
        return await _cache.GetStringAsync($"refresh:{userId}");
    }

    public async Task RevokeRefreshTokenAsync(Guid userId)
    {
        await _cache.RemoveAsync($"refresh:{userId}");
    }
}