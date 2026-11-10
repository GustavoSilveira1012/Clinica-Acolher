using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.Application.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Patient patient);
    Task SaveChangesAsync(CancellationToken ct);
}
