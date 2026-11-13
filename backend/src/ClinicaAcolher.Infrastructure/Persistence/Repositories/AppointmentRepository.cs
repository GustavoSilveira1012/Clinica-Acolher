using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class AppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AgendaItemResponse>> ListAgendaAsync(
        Guid professionalId, DateTimeOffset @from, DateTimeOffset to, CancellationToken ct) =>
        await (
            from a in db.Appointments
            join p in db.Patients on a.PatientId equals p.Id
            where a.ProfessionalId == professionalId && a.StartsAt < to && a.EndsAt > @from
            orderby a.StartsAt
            select new AgendaItemResponse(a.Id, a.PatientId, p.SocialName ?? p.FullName, a.StartsAt, a.EndsAt, a.Status)
        ).ToListAsync(ct);

    public void Add(Appointment appointment) => db.Appointments.Add(appointment);
}
