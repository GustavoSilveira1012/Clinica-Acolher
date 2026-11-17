using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.ClinicalNotes;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Application.ClinicalNotes;

public sealed class ClinicalNoteService(
    IClinicalNoteRepository notes,
    IRelationshipRepository relationships,
    IAppointmentRepository appointments,
    IFieldEncryptor encryptor,
    IAuditTrail audit,
    AccessControl access,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<ClinicalNoteResponse> CreateAsync(CreateClinicalNoteRequest r, CancellationToken ct)
    {
        var professionalId = access.RequireClinicalAuthor();

        var relationship = await relationships.GetByIdAsync(r.RelationshipId, ct);
        if (relationship is null || relationship.ProfessionalId != professionalId)
            throw new NotFoundException("Vínculo não encontrado.");

        if (r.AppointmentId is { } appointmentId)
        {
            var appointment = await appointments.GetByIdAsync(appointmentId, ct);
            if (appointment is null || appointment.RelationshipId != relationship.Id)
                throw new NotFoundException("Sessão não encontrada.");
        }

        ClinicalNote.EnsureValidContent(r.Content);
        var note = ClinicalNote.Start(relationship, r.AppointmentId, Seal(r.Content), clock.GetUtcNow());

        notes.Add(note);
        Record(ClinicalNoteAudit.Created, note);
        await unitOfWork.SaveChangesAsync(ct);
        return ToResponse(note);
    }

    public async Task<ClinicalNoteResponse> GetAsync(Guid id, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);

        // A leitura só é entregue depois de registrada: se a auditoria falhar, o texto não sai
        Record(ClinicalNoteAudit.Read, note);
        await unitOfWork.SaveChangesAsync(ct);
        return ToResponse(note);
    }

    public async Task<IReadOnlyList<ClinicalNoteSummary>> ListByPatientAsync(Guid patientId, CancellationToken ct)
    {
        var professionalId = access.RequireClinicalAuthor();
        if (!await relationships.ExistsAsync(professionalId, patientId, ct))
            throw new NotFoundException("Paciente não encontrado.");

        var list = await notes.ListByPatientAsync(professionalId, patientId, ct);

        audit.Record(new AuditEvent(ClinicalNoteAudit.Listed, "Patient", patientId, patientId));
        await unitOfWork.SaveChangesAsync(ct);
        return list;
    }

    public Task<ClinicalNoteResponse> SaveDraftAsync(Guid id, ClinicalNoteContentRequest r, CancellationToken ct) =>
        ChangeAsync(id, ClinicalNoteAudit.DraftSaved, (note, now) =>
        {
            ClinicalNote.EnsureValidContent(r.Content);
            note.SaveDraft(Seal(r.Content), now);
        }, ct);

    public Task<ClinicalNoteResponse> FinalizeAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, ClinicalNoteAudit.Finalized, (note, now) => note.FinalizeNote(now), ct);

    public Task<ClinicalNoteResponse> AddAddendumAsync(Guid id, ClinicalNoteContentRequest r, CancellationToken ct) =>
        ChangeAsync(id, ClinicalNoteAudit.AddendumAdded, (note, now) =>
        {
            ClinicalNote.EnsureValidContent(r.Content);
            note.AddAddendum(Seal(r.Content), now);
        }, ct);

    private async Task<ClinicalNoteResponse> ChangeAsync(
        Guid id, string action, Action<ClinicalNote, DateTimeOffset> change, CancellationToken ct)
    {
        var note = await LoadAsync(id, ct);
        await EnsureRelationshipOpenAsync(note, ct);

        change(note, clock.GetUtcNow());

        Record(action, note);
        await unitOfWork.SaveChangesAsync(ct);
        return ToResponse(note);
    }

    // RN-PRT-01: só o autor; para qualquer outra pessoa, a anotação "não existe"
    private async Task<ClinicalNote> LoadAsync(Guid id, CancellationToken ct)
    {
        var professionalId = access.RequireClinicalAuthor();
        var note = await notes.GetByIdAsync(id, ct);

        if (note is null || note.ProfessionalId != professionalId)
            throw new NotFoundException("Anotação não encontrada.");

        return note;
    }

    // RN-PER-04: com o vínculo encerrado, as anotações ficam só para leitura
    private async Task EnsureRelationshipOpenAsync(ClinicalNote note, CancellationToken ct)
    {
        var relationship = await relationships.GetByIdAsync(note.RelationshipId, ct);
        if (relationship is null || relationship.Status == RelationshipStatus.Ended)
            throw new DomainException("RN-PER-04",
                "O vínculo foi encerrado: as anotações ficam disponíveis só para leitura.");
    }

    private Func<string, byte[]> Seal(string content) => context => encryptor.Encrypt(content, context);

    private void Record(string action, ClinicalNote note) =>
        audit.Record(new AuditEvent(action, ClinicalNoteAudit.ResourceType, note.Id, note.PatientId));

    private ClinicalNoteResponse ToResponse(ClinicalNote note)
    {
        var current = note.CurrentRevision;

        return new ClinicalNoteResponse(
            note.Id, note.PatientId, note.AppointmentId, note.Status,
            encryptor.Decrypt(current.EncryptedContent, ClinicalNote.RevisionContext(note.Id, current.Number)),
            current.Number, note.CreatedAt, current.SavedAt, note.FinalizedAt,
            note.Addenda
                .OrderBy(a => a.Number)
                .Select(a => new ClinicalNoteAddendumResponse(
                    a.Number,
                    encryptor.Decrypt(a.EncryptedContent, ClinicalNote.AddendumContext(note.Id, a.Number)),
                    a.CreatedAt))
                .ToList());
    }
}