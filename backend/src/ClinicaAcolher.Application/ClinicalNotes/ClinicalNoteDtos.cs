using ClinicaAcolher.Domain.ClinicalNotes;

namespace ClinicaAcolher.Application.ClinicalNotes;

public sealed record CreateClinicalNoteRequest(Guid RelationshipId, Guid? AppointmentId, string Content);

public sealed record ClinicalNoteContentRequest(string Content);

/// <summary>Dados de controle, sem o texto: para a lista de anotações de um paciente.</summary>
public sealed record ClinicalNoteSummary(
    Guid Id,
    Guid? AppointmentId,
    ClinicalNoteStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSavedAt,
    DateTimeOffset? FinalizedAt,
    int RevisionCount,
    int AddendumCount);

public sealed record ClinicalNoteAddendumResponse(int Number, string Content, DateTimeOffset CreatedAt);

public sealed record ClinicalNoteResponse(
    Guid Id,
    Guid PatientId,
    Guid? AppointmentId,
    ClinicalNoteStatus Status,
    string Content,
    int RevisionNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSavedAt,
    DateTimeOffset? FinalizedAt,
    IReadOnlyList<ClinicalNoteAddendumResponse> Addenda);