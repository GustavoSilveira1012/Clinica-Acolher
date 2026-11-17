using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Domain.ClinicalNotes;

public sealed class ClinicalNote : Entity
{
    public const int MaxContentLength = 20_000;

    private readonly List<ClinicalNoteRevision> _revisions = [];
    private readonly List<ClinicalNoteAddendum> _addenda = [];

    public Guid RelationshipId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public ClinicalNoteStatus Status { get; private set; } = ClinicalNoteStatus.Draft;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? FinalizedAt { get; private set; }

    public IReadOnlyList<ClinicalNoteRevision> Revisions => _revisions;
    public IReadOnlyList<ClinicalNoteAddendum> Addenda => _addenda;

    public int NextRevisionNumber => _revisions.Count + 1;
    public int NextAddendumNumber => _addenda.Count + 1;
    public ClinicalNoteRevision CurrentRevision => _revisions.MaxBy(r => r.Number)!;

    private ClinicalNote() { }

    // RN-PRT-01: a anotação pertence ao vínculo paciente ↔ profissional, nunca à clínica
    public static ClinicalNote Start(
        TherapeuticRelationship relationship, Guid? appointmentId, byte[] encryptedContent, DateTimeOffset now)
    {
        // RN-PER-04: com o vínculo encerrado, o profissional só lê o que já escreveu
        if (relationship.Status == RelationshipStatus.Ended)
            throw new DomainException("RN-PER-04", "O vínculo foi encerrado: não é possível criar anotações novas.");

        var note = new ClinicalNote
        {
            RelationshipId = relationship.Id,
            PatientId = relationship.PatientId,
            ProfessionalId = relationship.ProfessionalId,
            AppointmentId = appointmentId,
            CreatedAt = now,
        };
        note.AddRevision(encryptedContent, now);
        return note;
    }

    // RN-PRT-04: cada salvamento explícito do rascunho cria uma versão nova
    public void SaveDraft(byte[] encryptedContent, DateTimeOffset now)
    {
        if (Status == ClinicalNoteStatus.Finalized)
            throw new DomainException("RN-PRT-03",
                "Uma anotação finalizada não pode ser alterada. Para corrigir, acrescente um adendo.");

        AddRevision(encryptedContent, now);
    }

    // RN-PRT-03: depois de finalizada, a anotação é imutável
    public void FinalizeNote(DateTimeOffset now)
    {
        if (Status == ClinicalNoteStatus.Finalized)
            throw new DomainException("RN-PRT-03", "Esta anotação já foi finalizada.");

        Status = ClinicalNoteStatus.Finalized;
        FinalizedAt = now;
    }

    // RN-PRT-03: correção depois da finalização, sem alterar o original
    public void AddAddendum(byte[] encryptedContent, DateTimeOffset now)
    {
        if (Status != ClinicalNoteStatus.Finalized)
            throw new DomainException("RN-PRT-03",
                "Adendos só existem em anotações finalizadas. Enquanto for rascunho, edite o próprio texto.");

        _addenda.Add(new ClinicalNoteAddendum(Id, NextAddendumNumber, encryptedContent, now));
    }

    // RN-PRT-10: validado antes de cifrar, porque depois o texto deixa de ser legível
    public static void EnsureValidContent(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
            throw new DomainException("RN-PRT-10", "A anotação precisa ter algum texto.");

        if (plaintext.Length > MaxContentLength)
            throw new DomainException("RN-PRT-10", $"A anotação pode ter no máximo {MaxContentLength} caracteres.");
    }

    private void AddRevision(byte[] encryptedContent, DateTimeOffset now) =>
        _revisions.Add(new ClinicalNoteRevision(Id, NextRevisionNumber, encryptedContent, now));
}