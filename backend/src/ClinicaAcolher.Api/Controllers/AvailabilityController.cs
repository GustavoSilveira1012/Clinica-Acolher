using ClinicaAcolher.Application.Availability;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
public sealed class AvailabilityController(AvailabilityService service) : ControllerBase
{
    [HttpGet("/api/v1/professionals/{professionalId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> Get(Guid professionalId, CancellationToken ct) =>
        Ok(await service.GetAsync(professionalId, ct));

    [HttpPut("/api/v1/professionals/{professionalId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> Save(
        Guid professionalId, UpdateAvailabilityRequest request, CancellationToken ct) =>
        Ok(await service.SaveAsync(professionalId, request, ct));

    [HttpGet("/api/v1/professionals/{professionalId:guid}/availability/check")]
    public async Task<ActionResult<AvailabilityCheckResponse>> Check(
        Guid professionalId, [FromQuery] DateTimeOffset startsAt, [FromQuery] int durationMinutes, CancellationToken ct) =>
        Ok(await service.CheckAsync(professionalId, startsAt, durationMinutes, ct));

    [HttpGet("/api/v1/professionals/{professionalId:guid}/availability-blocks")]
    public async Task<ActionResult<IReadOnlyList<BlockResponse>>> ListBlocks(
        Guid professionalId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken ct) =>
        Ok(await service.ListBlocksAsync(professionalId, from, to, ct));

    [HttpPost("/api/v1/professionals/{professionalId:guid}/availability-blocks")]
    public async Task<ActionResult<BlockCreatedResponse>> CreateBlock(
        Guid professionalId, CreateBlockRequest request, CancellationToken ct)
    {
        var created = await service.CreateBlockAsync(professionalId, request, ct);
        return CreatedAtAction(nameof(ListBlocks), new { professionalId }, created);
    }

    [HttpDelete("/api/v1/availability-blocks/{blockId:guid}")]
    public async Task<IActionResult> DeleteBlock(Guid blockId, CancellationToken ct)
    {
        await service.DeleteBlockAsync(blockId, ct);
        return NoContent();
    }
}