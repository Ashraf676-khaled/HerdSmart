// Web/Realtime/SignalRNotifier.cs
using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Realtime;

public class SignalRNotifier(IHubContext<NotificationHub> hubContext) : IRealtimeNotifier
{
    public async Task NotifyTenantAsync(
        Ulid tenantId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(NotificationHub.GroupName(tenantId.ToString()))
            .SendAsync(eventName, payload, cancellationToken);
    }
}