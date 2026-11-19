using ClinicaAcolher.Application.Availability;
using ClinicaAcolher.Domain.Availability;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class AvailabilityRepository(AppDbContext db) : IAvailabilityRepository
{
    public Task<ProfessionalAvailability?> GetByProfessionalAsync(Guid professionalId, CancellationToken ct) =>
        db.ProfessionalAvailabilities.FirstOrDefaultAsync(a => a.ProfessionalId == professionalId, ct);

    public void Add(ProfessionalAvailability availability) => db.ProfessionalAvailabilities.Add(availability);

    public async Task<IReadOnlyList<AvailabilityBlock>> ListBlocksAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        await db.AvailabilityBlocks
            .AsNoTracking()
            .Where(b => b.ProfessionalId == professionalId && b.StartsAt < to && b.EndsAt > from)
            .OrderBy(b => b.StartsAt)
            .ToListAsync(ct);

    public Task<AvailabilityBlock?> GetBlockAsync(Guid id, CancellationToken ct) =>
        db.AvailabilityBlocks.FirstOrDefaultAsync(b => b.Id == id, ct);

    public void AddBlock(AvailabilityBlock block) => db.AvailabilityBlocks.Add(block);

    public void RemoveBlock(AvailabilityBlock block) => db.AvailabilityBlocks.Remove(block);
}