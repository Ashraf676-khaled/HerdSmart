// Api/Controllers/TelemetryController.cs
using Application.Features.Telemetry.Commands.IngestTelemetryReading;
using Application.Features.Telemetry.Jobs;
using HerdSmart.Domain.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/telemetry")]
[EnableRateLimiting("telemetry-limit")]
public class TelemetryController(IConfiguration configuration) : ApiControllerBase
{
    [HttpPost("ingest")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TelemetryIngestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TelemetryIngestResult>> Ingest(
        [FromHeader(Name = "X-Api-Key")] string apiKey,
        [FromBody] IngestTelemetryRequest request,
        CancellationToken cancellationToken)
    {
        if (apiKey != configuration["IoT:ApiKey"])
        {
            return Unauthorized();
        }

        var result = await Sender.Send(
            new IngestTelemetryReadingCommand(
                request.CattleId,
                request.SensorType,
                request.Value,
                request.RecordedAt ?? DateTimeOffset.UtcNow),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("cleanup")]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cleanup(
        [FromServices] CleanupResolvedTelemetryAlertsJob job,
        CancellationToken cancellationToken)
    {
        await job.ExecuteAsync(cancellationToken);
        return NoContent();
    }
}

public record IngestTelemetryRequest(
    Ulid CattleId, SensorType SensorType, double Value, DateTimeOffset? RecordedAt);