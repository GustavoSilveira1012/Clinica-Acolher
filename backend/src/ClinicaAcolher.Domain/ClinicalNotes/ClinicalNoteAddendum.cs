using ClinicaAcolher.Domain.Common;

namespace ClinicaAcolher.Domain.ClinicalNotes;

public sealed class ClinicalNoteAddendum : Entity
{
    public Guid NoteId { get; private set; }
    public int Number { get; private set; }
    public byte[] EncryptedContent { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }

    private ClinicalNoteAddendum() { }

    internal ClinicalNoteAddendum(Guid noteId, int number, byte[] encryptedContent, DateTimeOffset createdAt)
    {
        if (encryptedContent.Length == 0)
            throw new DomainException("RN-PRT-02", "O conteúdo do adendo não pode chegar vazio.");

        NoteId = noteId;
        Number = number;
        EncryptedContent = encryptedContent.ToArray();
        CreatedAt = createdAt;
    }
}