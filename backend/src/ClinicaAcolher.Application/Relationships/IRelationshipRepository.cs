using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Application.Relationships;

public interface IRelationshipRepository
{
    Task<TherapeuticRelationship?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> HasOpenAsync(Guid patientId, Guid professionalId, CancellationToken ct);
    Task<IReadOnlyList<TherapeuticRelationship>> ListByPatientAsync(Guid patientId, CancellationToken ct);
    Task<IReadOnlyList<ActiveRelationshipResponse>> ListActiveByProfessionalAsync(
        Guid professionalId, CancellationToken ct);
    void Add(TherapeuticRelationship relationship);
}