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
        AppointmentResponse.From(await LoadAsync(id, ct));

    public Task<IReadOnlyList<AgendaItemResponse>> GetAgendaAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        appointments.ListAgendaAsync(professionalId, from.ToUniversalTime(), to.ToUniversalTime(), ct);

    public Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (a, now) => a.Confirm(now), ct);

    public Task<AppointmentResponse> CompleteAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (a, now) => a.Complete(now), ct);

    public Task<AppointmentResponse> MarkNoShowAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (a, now) => a.MarkNoShow(now), ct);

    public Task<AppointmentResponse> CancelAsync(Guid id, CancelAppointmentRequest r, CancellationToken ct) =>
        ChangeAsync(id, (a, now) => a.Cancel(r.By, r.Reason, now), ct);

    public async Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest r, CancellationToken ct)
    {
        var original = await LoadAsync(id, ct);
        var relationship = await relationships.GetByIdAsync(original.RelationshipId, ct)
            ?? throw new NotFoundException("Vínculo não encontrado.");

        var now = clock.GetUtcNow();
        var duration = r.DurationMinutes is { } minutes ? TimeSpan.FromMinutes(minutes) : original.Duration;
        var replacement = Appointment.Schedule(relationship, r.StartsAt, duration, now);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 1. A sessão original vira "Remarcada" e libera o horário dela
            original.MarkRescheduled(replacement.Id, now);
            await unitOfWork.SaveChangesAsync(ct);

            // 2. A nova ocupa o novo horário. Se conflitar, a transação desfaz o passo 1
            appointments.Add(replacement);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct);

        return AppointmentResponse.From(replacement);
    }

    private async Task<AppointmentResponse> ChangeAsync(
        Guid id, Action<Appointment, DateTimeOffset> change, CancellationToken ct)
    {
        var appointment = await LoadAsync(id, ct);
        change(appointment, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return AppointmentResponse.From(appointment);
    }

    private async Task<Appointment> LoadAsync(Guid id, CancellationToken ct) =>
        await appointments.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sessão não encontrada.");
}
