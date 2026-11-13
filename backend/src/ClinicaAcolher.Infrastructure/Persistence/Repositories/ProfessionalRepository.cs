using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Professionals;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalRepository(AppDbContext db) : IProfessionalRepository
{
    public Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Professionals.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Professional>> ListAsync(CancellationToken ct) =>
        await db.Professionals
            .AsNoTracking()
            .OrderBy(p => p.ProfessionalName)
            .ThenBy(p => p.Id)
            .Take(200)
            .ToListAsync(ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        db.Professionals.AnyAsync(p => p.Email == email, ct);

    public void Add(Professional professional) => db.Professionals.Add(professional);
}