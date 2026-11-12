using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[Route("api/v1/patients")]
public sealed class PatientsController(PatientService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PatientResponse>> Create(CreatePatientRequest request, CancellationToken ct)
    {
        var patient = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id}, patient);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id, CancellationToken ct)
    {
        var patient = await service.GetByIdAsync(id, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<PatientListItem>>> Search(PatientSearchRequest request, CancellationToken ct) =>
        Ok(await service.SearchAsync(request, ct));


    [HttpPut("{id:guid}")]
public async Task<ActionResult<PatientResponse>> Update(Guid id, UpdatePatientRequest request, CancellationToken ct) =>
    OkOrNotFound(await service.UpdateAsync(id, request, ct));

[HttpPost("{id:guid}/deactivate")]
public async Task<ActionResult<PatientResponse>> Deactivate(Guid id, DeactivatePatientRequest request, CancellationToken ct) =>
    OkOrNotFound(await service.DeactivateAsync(id, request, ct));

[HttpPost("{id:guid}/reactivate")]
public async Task<ActionResult<PatientResponse>> Reactivate(Guid id, CancellationToken ct) =>
    OkOrNotFound(await service.ReactivateAsync(id, ct));

private ActionResult<PatientResponse> OkOrNotFound(PatientResponse? patient) =>
    patient is null ? NotFound() : Ok(patient);
}
