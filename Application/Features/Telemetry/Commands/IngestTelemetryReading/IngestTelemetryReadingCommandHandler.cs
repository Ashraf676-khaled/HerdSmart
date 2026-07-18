using Application.Common.Exceptions;
using Application.Common.Interfaces;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Telemetry.Commands.IngestTelemetryReading;

public class IngestTelemetryReadingCommandHandler(
    IApplicationDbContext context,
    IRealtimeNotifier notifier,
    ILogger<IngestTelemetryReadingCommandHandler> logger,
    IHeartbeatTracker heartbeatTracker
    )
    : IRequestHandler<IngestTelemetryReadingCommand, TelemetryIngestResult>
{
    public async Task<TelemetryIngestResult> Handle(
        IngestTelemetryReadingCommand request, CancellationToken cancellationToken)
    {
        await heartbeatTracker.RecordAsync("global", cancellationToken);
        var cattle = await context.Cattle
            .FirstOrDefaultAsync(c => c.Id == request.CattleId, cancellationToken)
            ?? throw new NotFoundException($"Cattle with ID {request.CattleId} was not found.");

        var (isAbnormal, severity, message) = Evaluate(request.SensorType, request.Value);

        if (!isAbnormal)
        {
            // Reading is normal, no alert needed
            return new TelemetryIngestResult(false, null);
        }

        var alert = new TelemetryAlert
        {
            TenantId = cattle.TenantId,
            CattleId = cattle.Id,
            SensorType = request.SensorType,
            Value = request.Value,
            Message = message,
            Severity = severity,
            IsResolved = false
        };

        context.TelemetryAlerts.Add(alert);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogWarning("Telemetry alert created for Cattle {CattleId}: {Message}", cattle.Id, message);

        // Send real-time notification in English to the specific tenant group
        await notifier.NotifyTenantAsync(
            cattle.TenantId,
            "TelemetryAlertCreated",
            new
            {
                alert.Id,
                CattleId = cattle.Id,
                CattleTagNumber = cattle.TagNumber,
                SensorType = alert.SensorType.ToString(),
                alert.Value,
                alert.Message,
                Severity = alert.Severity.ToString(),
                alert.CreatedAt
            },
            cancellationToken);

        return new TelemetryIngestResult(true, alert.Id);
    }

    private static (bool IsAbnormal, AlertSeverity Severity, string Message) Evaluate(
        SensorType sensorType, double value) => sensorType switch
        {
            SensorType.Temperature => value switch
            {
                > 41.0 => (true, AlertSeverity.Critical, $"Critical high temperature detected: {value}°C"),
                > 40.0 => (true, AlertSeverity.High, $"High temperature detected: {value}°C"),
                > 39.5 => (true, AlertSeverity.Medium, $"Slight temperature elevation: {value}°C"),
                < 37.5 => (true, AlertSeverity.Medium, $"Low temperature detected: {value}°C"),
                _ => (false, default, string.Empty)
            },
            SensorType.ActivityLevel => value switch
            {
                < 10 => (true, AlertSeverity.High, $"Severe drop in activity level: {value}"),
                < 20 => (true, AlertSeverity.Medium, $"Noticeable decrease in activity level: {value}"),
                _ => (false, default, string.Empty)
            },
            _ => (false, default, string.Empty)
        };
}