using ClinicaAcolher.Application.Relationships;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[Route("api/v1/relationships")]
public sealed class RelationshipsController(RelationshipService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RelationshipResponse>> Start(StartRelationshipRequest request, CancellationToken ct)
    {
        var relationship = await service.StartAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = relationship.Id }, relationship);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RelationshipResponse>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpGet("/api/v1/patients/{patientId:guid}/relationships")]
    public async Task<ActionResult<IReadOnlyList<RelationshipResponse>>> ListByPatient(Guid patientId, CancellationToken ct) =>
        Ok(await service.ListByPatientAsync(patientId, ct));

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<RelationshipResponse>> Suspend(Guid id, CancellationToken ct) =>
        Ok(await service.SuspendAsync(id, ct));

    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<RelationshipResponse>> Resume(Guid id, CancellationToken ct) =>
        Ok(await service.ResumeAsync(id, ct));

    [HttpPost("{id:guid}/end")]
    public async Task<ActionResult<RelationshipResponse>> End(Guid id, EndRelationshipRequest request, CancellationToken ct) =>
        Ok(await service.EndAsync(id, request, ct));
}