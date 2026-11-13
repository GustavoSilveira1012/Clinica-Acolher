using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public sealed record ScheduleAppointmentRequest(Guid RelationshipId, DateTimeOffset StartsAt, int DurationMinutes);

public sealed record AppointmentResponse(
    Guid Id,
    Guid RelationshipId,
    Guid PatientId,
    Guid ProfessionalId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status,
    CancellationSource? CancelledBy,
    string? CancellationReason,
    Guid? RescheduledToId)
{
    public static AppointmentResponse From(Appointment a) => new(
        a.Id, a.RelationshipId, a.PatientId, a.ProfessionalId, a.StartsAt, a.EndsAt,
        a.Status, a.CancelledBy, a.CancellationReason, a.RescheduledToId);
}

public sealed record AgendaItemResponse(
    Guid Id,
    Guid PatientId,
    string PatientDisplayName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status);