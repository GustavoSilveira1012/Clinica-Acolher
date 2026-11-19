namespace ClinicaAcolher.Application.ClinicalNotes;

// RN-PRT-05: as ações registradas na trilha de auditoria
public static class ClinicalNoteAudit
{
    public const string ResourceType = "ClinicalNote";

    public const string Created = "ClinicalNote.Created";
    public const string Read = "ClinicalNote.Read";
    public const string Listed = "ClinicalNote.Listed";
    public const string DraftSaved = "ClinicalNote.DraftSaved";
    public const string Finalized = "ClinicalNote.Finalized";
    public const string AddendumAdded = "ClinicalNote.AddendumAdded";
}