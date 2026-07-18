// Application/Common/Interfaces/IRealtimeNotifier.cs
namespace Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyTenantAsync(Ulid tenantId, string eventName, object payload, CancellationToken cancellationToken = default);
}