using ClinicaAcolher.Application.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[Route("api/v1/series")]
public sealed class SeriesController(SeriesService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SeriesResponse>> Create(CreateSeriesRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SeriesResponse>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpPost("{id:guid}/end")]
    public async Task<ActionResult<SeriesResponse>> End(Guid id, EndSeriesRequest request, CancellationToken ct) =>
        Ok(await service.EndAsync(id, request, ct));
}