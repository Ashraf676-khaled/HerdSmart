// Api/Controllers/VaccinationsController.cs
using Application.Features.Vaccinations;
using Application.Features.Vaccinations.Commands.AdministerVaccination;
using Application.Features.Vaccinations.Commands.CancelVaccination;
using Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;
using Application.Features.Vaccinations.Commands.RescheduleVaccination;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace HerdSmart.Api.Controllers;

[Route("api/vaccinations")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class VaccinationsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<VaccinationScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<VaccinationScheduleResponse>>> GetAll(
        [FromQuery] GetAllVaccinationsQuery query, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("cattle/{cattleId}")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<VaccinationScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<VaccinationScheduleResponse>>> GetByCattle(
        Ulid cattleId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetVaccinationsByCattleQuery(cattleId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(PaginatedResult<VaccinationScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<VaccinationScheduleResponse>>> GetOverdue(
        [FromQuery] GetOverdueVaccinationsQuery query, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Owner,Vet,Worker")]
    [ProducesResponseType(typeof(IEnumerable<VaccinationScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VaccinationScheduleResponse>>> GetUpcoming(
        [FromQuery] GetUpcomingVaccinationsQuery query, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(VaccinationScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VaccinationScheduleResponse>> Create(
        CreateVaccinationScheduleCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByCattle), new { cattleId = result.CattleId }, result);
    }

    [HttpPost("{id}/administer")]
    [Authorize(Roles = "Vet")]
    [ProducesResponseType(typeof(VaccinationScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VaccinationScheduleResponse>> Administer(
        Ulid id, [FromBody] DateTimeOffset? administeredDate, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new AdministerVaccinationCommand(id, administeredDate), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/reschedule")]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(typeof(VaccinationScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VaccinationScheduleResponse>> Reschedule(
        Ulid id, [FromBody] DateTimeOffset newScheduledDate, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new RescheduleVaccinationCommand(id, newScheduledDate), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Vet")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Ulid id, CancellationToken cancellationToken)
    {
        await Sender.Send(new CancelVaccinationCommand(id), cancellationToken);
        return NoContent();
    }
}