using ClinicaAcolher.Application.Professionals;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[Route("api/v1/professionals")]
public sealed class ProfessionalsController(ProfessionalService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProfessionalResponse>> Create(CreateProfessionalRequest request, CancellationToken ct)
    {
        var professional = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = professional.Id }, professional);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProfessionalResponse>> GetById(Guid id, CancellationToken ct) =>
        OkOrNotFound(await service.GetByIdAsync(id, ct));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ProfessionalResponse>> Approve(Guid id, CancellationToken ct) =>
        OkOrNotFound(await service.ApproveAsync(id, ct));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProfessionalResponse>> Reject(Guid id, CancellationToken ct) =>
        OkOrNotFound(await service.RejectAsync(id, ct));

    private ActionResult<ProfessionalResponse> OkOrNotFound(ProfessionalResponse? professional) =>
        professional is null ? NotFound() : Ok(professional);
}