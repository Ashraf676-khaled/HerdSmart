using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Web.Realtime;

public class CacheHeartbeatTracker(IDistributedCache cache) : IHeartbeatTracker
{
    private const string KeyPrefix = "heartbeat:";

    public async Task RecordAsync(string key, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTimeOffset.UtcNow.ToString("O");
        await cache.SetStringAsync($"{KeyPrefix}{key}", currentTime, cancellationToken);
    }

    public async Task<DateTimeOffset?> GetLastSeenAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync($"{KeyPrefix}{key}", cancellationToken);

        if (!string.IsNullOrEmpty(value) && DateTimeOffset.TryParse(value, out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }
}