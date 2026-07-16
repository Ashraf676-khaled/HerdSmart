// Api/Controllers/VaccinesController.cs
using Application.Features.Vaccines;
using Application.Features.Vaccines.Commands.CreateVaccine;
using Application.Features.Vaccines.Commands.DeleteVaccine;
using Application.Features.Vaccines.Commands.UpdateVaccine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/vaccines")]
[EnableRateLimiting("global-limit")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class VaccinesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<VaccineResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<VaccineResponse>>> GetAll(
        [FromQuery] GetAllVaccinesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(VaccineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VaccineResponse>> GetById(
        Ulid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVaccineByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(typeof(VaccineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VaccineResponse>> Create(
        CreateVaccineCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(typeof(VaccineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VaccineResponse>> Update(
        Ulid id, UpdateVaccineCommand request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateVaccineCommand(id, request.Name, request.TargetAgeInMonths,
                request.Dosage, request.IntervalInDays),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Ulid id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteVaccineCommand(id), cancellationToken);
        return NoContent();
    }
}