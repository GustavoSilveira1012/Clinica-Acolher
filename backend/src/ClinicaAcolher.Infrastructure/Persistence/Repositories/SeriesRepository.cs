using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class SeriesRepository(AppDbContext db) : ISeriesRepository
{
    public Task<AppointmentSeries?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AppointmentSeries.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Guid>> ListDueIdsAsync(DateOnly until, CancellationToken ct) =>
        await (
            from s in db.AppointmentSeries
            join r in db.TherapeuticRelationships on s.RelationshipId equals r.Id
            where r.Status == RelationshipStatus.Active
               && (s.GeneratedUntil == null || s.GeneratedUntil < until)
               && (s.EndsOn == null || s.GeneratedUntil == null || s.GeneratedUntil < s.EndsOn)
            orderby s.GeneratedUntil, s.Id
            select s.Id
        ).Take(500).ToListAsync(ct);

    public void Add(AppointmentSeries series) => db.AppointmentSeries.Add(series);
}
