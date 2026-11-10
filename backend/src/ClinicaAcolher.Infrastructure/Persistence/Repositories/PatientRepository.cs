using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class PatientRepository(AppDbContext db) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Add(Patient patient) => db.Patients.Add(patient);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
