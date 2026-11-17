using ClinicaAcolher.Domain.Common;

namespace ClinicaAcolher.Domain.ClinicalNotes;

public sealed class ClinicalNoteRevision : Entity
{
    public Guid NoteId { get; private set; }
    public int Number { get; private set; }
    public byte[] EncryptedContent { get; private set; } = [];
    public DateTimeOffset SavedAt { get; private set; }

    private ClinicalNoteRevision() { }

    internal ClinicalNoteRevision(Guid noteId, int number, byte[] encryptedContent, DateTimeOffset savedAt)
    {
        if (encryptedContent.Length == 0)
            throw new DomainException("RN-PRT-02", "O conteúdo da anotação não pode chegar vazio.");

        NoteId = noteId;
        Number = number;
        EncryptedContent = encryptedContent.ToArray();
        SavedAt = savedAt;
    }
}