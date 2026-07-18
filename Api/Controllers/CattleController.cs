// Api/Controllers/CattleController.cs
using Application.Features.Cattle.Commands.CreateCattle;
using Application.Features.Cattle.Commands.UpdateCattle;
using Application.Features.Cattle.Dtos;
using HerdSmart.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/[controller]")]
[EnableRateLimiting("global-limit")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class CattleController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<CattleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<CattleResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] CattleStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetAllCattleQuery(page, pageSize, search, status),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(CattleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CattleResponse>> GetById(
        Ulid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetCattleByIdQuery(id),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}/history")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(CattleHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CattleHistoryResponse>> GetHistory(
        Ulid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetCattleHistoryQuery(id),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(CattleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CattleResponse>> Create(
        CreateCattleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(CattleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CattleResponse>> Update(
    Ulid id,
    UpdateCattleCommand request,  
    CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateCattleCommand(
                id,
                request.TagNumber,
                request.Breed,
                request.BirthDate,
                request.FatherTagNumber,
                request.MotherTagNumber),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(CattleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CattleResponse>> UpdateStatus(
        Ulid id,
        UpdateCattleStatusCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command with { Id = id },
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Ulid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteCattleCommand(id),
            cancellationToken);
        return NoContent();
    }
}