using ClinicaAcolher.Application.ClinicalNotes;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ClinicalNotesController(ClinicalNoteService service) : ControllerBase
{
    [HttpPost("/api/v1/clinical-notes")]
    public async Task<ActionResult<ClinicalNoteResponse>> Create(CreateClinicalNoteRequest request, CancellationToken ct)
    {
        var note = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
    }

    [HttpGet("/api/v1/clinical-notes/{id:guid}")]
    public async Task<ActionResult<ClinicalNoteResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));

    [HttpGet("/api/v1/patients/{patientId:guid}/clinical-notes")]
    public async Task<ActionResult<IReadOnlyList<ClinicalNoteSummary>>> ListByPatient(Guid patientId, CancellationToken ct) =>
        Ok(await service.ListByPatientAsync(patientId, ct));

    [HttpPost("/api/v1/clinical-notes/{id:guid}/revisions")]
    public async Task<ActionResult<ClinicalNoteResponse>> SaveDraft(Guid id, ClinicalNoteContentRequest request, CancellationToken ct) =>
        Ok(await service.SaveDraftAsync(id, request, ct));

    [HttpPost("/api/v1/clinical-notes/{id:guid}/finalize")]
    public async Task<ActionResult<ClinicalNoteResponse>> Finalize(Guid id, CancellationToken ct) =>
        Ok(await service.FinalizeAsync(id, ct));

    [HttpPost("/api/v1/clinical-notes/{id:guid}/addenda")]
    public async Task<ActionResult<ClinicalNoteResponse>> AddAddendum(Guid id, ClinicalNoteContentRequest request, CancellationToken ct) =>
        Ok(await service.AddAddendumAsync(id, request, ct));
}