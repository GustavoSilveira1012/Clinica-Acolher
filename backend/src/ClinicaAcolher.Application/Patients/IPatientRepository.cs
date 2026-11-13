using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.Application.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Patient patient);
    Task<(IReadOnlyList<Patient> Items, int Total)> SearchAsync(
        string? search, bool? active, int skip, int take, CancellationToken ct
    );
}
