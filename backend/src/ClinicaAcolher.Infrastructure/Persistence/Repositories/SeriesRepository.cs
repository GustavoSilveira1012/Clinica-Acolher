using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class SeriesRepository(AppDbContext db) : ISeriesRepository
{
    public Task<AppointmentSeries?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AppointmentSeries.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Guid>> ListDueIdsAsync(DateOnly until, CancellationToken ct) =>
        await db.AppointmentSeries
            .Where(s => (s.GeneratedUntil == null || s.GeneratedUntil < until)
                     && (s.EndsOn == null || s.GeneratedUntil == null || s.GeneratedUntil < s.EndsOn))
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .Take(500)
            .ToListAsync(ct);

    public void Add(AppointmentSeries series) => db.AppointmentSeries.Add(series);
}