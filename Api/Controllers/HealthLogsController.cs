// Api/Controllers/HealthLogsController.cs
using Application.Features.HealthLog.Commands.CreateHealthLog;
using Application.Features.HealthLogs.Commands.UpdateHealthLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/health-logs")]
[EnableRateLimiting("global-limit")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class HealthLogsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<HealthLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<HealthLogResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isContagious = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetAllHealthLogsQuery(page, pageSize, isContagious),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("cattle/{cattleId}")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<HealthLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<HealthLogResponse>>> GetByCattle(
        Ulid cattleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetHealthLogsByCattleQuery(cattleId, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(typeof(HealthLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthLogResponse>> Create(
        CreateHealthLogCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetByCattle),
            new { cattleId = result.CattleId },
            result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(typeof(HealthLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthLogResponse>> Update(
        Ulid id,
        UpdateHealthLogCommand request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateHealthLogCommand(
                id,
                request.Diagnosis,
                request.TreatmentPlan,
                request.VetNotes),
            cancellationToken);
        return Ok(result);
    }
}