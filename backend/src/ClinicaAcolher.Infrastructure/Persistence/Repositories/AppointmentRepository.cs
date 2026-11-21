using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Portal;
using ClinicaAcolher.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class AppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AgendaEntry>> ListAgendaAsync(
        Guid professionalId,
        DateTimeOffset @from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        return await (
            from a in db.Appointments.AsNoTracking()
            join p in db.Patients on a.PatientId equals p.Id
            where a.ProfessionalId == professionalId
                && a.StartsAt < to
                && a.EndsAt > @from
            orderby a.StartsAt
            select new AgendaEntry(a, p.SocialName ?? p.FullName)
        ).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> ListActiveBySeriesAfterAsync(
        Guid seriesId, DateTimeOffset after, CancellationToken ct) =>
        await db.Appointments
            .Where(a => a.SeriesId == seriesId
                     && a.StartsAt > after
                     && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
            .ToListAsync(ct);

    public Task<int> CountActiveInPeriodAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        db.Appointments.CountAsync(a =>
            a.ProfessionalId == professionalId
            && a.StartsAt < to && a.EndsAt > from
            && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed), ct);

    public async Task<IReadOnlyList<PortalAppointment>> ListForPatientAsync(
        Guid patientId, DateTimeOffset @from, DateTimeOffset to, CancellationToken ct) =>
        await (
            from a in db.Appointments.AsNoTracking()
            join p in db.Professionals on a.ProfessionalId equals p.Id
            where a.PatientId == patientId
               && a.StartsAt >= @from && a.StartsAt < to
               && a.Status != AppointmentStatus.Rescheduled
            orderby a.StartsAt
            select new PortalAppointment(a.Id, a.StartsAt, a.EndsAt, a.Status, p.ProfessionalName)
        ).ToListAsync(ct);

    public void Add(Appointment appointment) => db.Appointments.Add(appointment);
}
