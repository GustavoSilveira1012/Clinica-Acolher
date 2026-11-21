using ClinicaAcolher.Domain.PatientAccess;

namespace ClinicaAcolher.Application.PatientAccess;

public interface IPatientInvitationRepository
{
    Task<PatientInvitation?> FindByTokenHashAsync(byte[] tokenHash, CancellationToken ct);
    Task<IReadOnlyList<PatientInvitation>> ListOpenByPatientAsync(Guid patientId, CancellationToken ct);
    void Add(PatientInvitation invitation);
}