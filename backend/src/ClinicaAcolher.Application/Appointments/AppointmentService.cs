using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public sealed class AppointmentService(
    IRelationshipRepository relationships,
    IAppointmentRepository appointments,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<AppointmentResponse> ScheduleAsync(ScheduleAppointmentRequest r, CancellationToken ct)
    {
        var relationship = await relationships.GetByIdAsync(r.RelationshipId, ct)
            ?? throw new NotFoundException("Vínculo não encontrado.");

        var appointment = Appointment.Schedule(
            relationship, r.StartsAt, TimeSpan.FromMinutes(r.DurationMinutes), clock.GetUtcNow());

        appointments.Add(appointment);
        await unitOfWork.SaveChangesAsync(ct);

        return AppointmentResponse.From(appointment);
    }

    public async Task<AppointmentResponse> GetByIdAsync(Guid id, CancellationToken ct) =>
        AppointmentResponse.From(
            await appointments.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sessão não encontrada."));

    public Task<IReadOnlyList<AgendaItemResponse>> GetAgendaAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        appointments.ListAgendaAsync(professionalId, from.ToUniversalTime(), to.ToUniversalTime(), ct);
}