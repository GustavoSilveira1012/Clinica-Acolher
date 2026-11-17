using ClinicaAcolher.Domain.ClinicalNotes;

namespace ClinicaAcolher.Application.ClinicalNotes;

public interface IClinicalNoteRepository
{
    /// <summary>Carrega a anotação com todas as versões e adendos.</summary>
    Task<ClinicalNote?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<ClinicalNoteSummary>> ListByPatientAsync(Guid professionalId, Guid patientId, CancellationToken ct);

    void Add(ClinicalNote note);
}