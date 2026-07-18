// Api/Controllers/MilkController.cs
using Application.Features.MilkLogs.Commands.CreateMilkLog;
using Application.Features.MilkLogs.Commands.UpdateMilkLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/milk")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class MilkController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<MilkLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<MilkLogResponse>>> GetAll(
        [FromQuery] GetAllMilkLogsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("cattle/{cattleId}")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<MilkLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<MilkLogResponse>>> GetByCattle(
        Ulid cattleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetMilkLogsByCattleQuery(cattleId, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(MilkSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MilkSummaryResponse>> GetSummary(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetMilkSummaryQuery(from, to),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-producers")]
    [Authorize(Roles = "Owner,Vet")] 
    [ProducesResponseType(typeof(IEnumerable<TopProducerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TopProducerResponse>>> GetTopProducers(
        [FromQuery] GetTopProducersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Vet,Worker")] 
    [ProducesResponseType(typeof(MilkLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MilkLogResponse>> Create(
        CreateMilkLogCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetByCattle),
            new { cattleId = result.CattleId },
            result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Worker")] 
    [ProducesResponseType(typeof(MilkLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MilkLogResponse>> Update(
        Ulid id,
        UpdateMilkLogCommand request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateMilkLogCommand(id, request.AmountInLiters, request.Shift),
            cancellationToken);
        return Ok(result);
    }
}