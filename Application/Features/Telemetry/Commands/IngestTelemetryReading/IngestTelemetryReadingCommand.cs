// Application/Features/Telemetry/Commands/IngestTelemetryReading/IngestTelemetryReadingCommand.cs
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.Telemetry.Commands.IngestTelemetryReading;

public record IngestTelemetryReadingCommand(
    Ulid CattleId,
    SensorType SensorType,
    double Value,
    DateTimeOffset RecordedAt) : IRequest<TelemetryIngestResult>;

public record TelemetryIngestResult(bool AlertCreated, Ulid? AlertId);