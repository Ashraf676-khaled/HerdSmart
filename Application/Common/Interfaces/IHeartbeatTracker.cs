// Application/Common/Interfaces/IHeartbeatTracker.cs
namespace Application.Common.Interfaces;

public interface IHeartbeatTracker
{
    Task RecordAsync(string key, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> GetLastSeenAsync(string key, CancellationToken cancellationToken = default);
}