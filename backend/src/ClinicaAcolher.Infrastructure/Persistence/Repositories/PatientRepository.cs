using System.Reflection.Metadata.Ecma335;
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

    public async Task<(IReadOnlyList<Patient> Items, int Total)> SearchAsync(
    string? search, bool? active, int skip, int take, CancellationToken ct)
{
    var query = db.Patients.AsNoTracking();

    if (active is not null)
        query = query.Where(p => p.Active == active);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = $"%{search.Trim()}%";
        query = query.Where(p =>
            EF.Functions.ILike(p.FullName, term) ||
            (p.SocialName != null && EF.Functions.ILike(p.SocialName, term)));
    }

    var total = await query.CountAsync(ct);

    var items = await query
        .OrderBy(p => p.FullName)
        .ThenBy(p => p.Id)
        .Skip(skip)
        .Take(take)
        .ToListAsync(ct);

    return (items, total);
}
}
