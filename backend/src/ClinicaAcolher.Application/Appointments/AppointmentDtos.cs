using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Attendance;

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

public sealed record AgendaEntry(Appointment Appointment, string PatientDisplayName);

public sealed record AgendaItemResponse(
    Guid Id,
    Guid PatientId,
    string PatientDisplayName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status,
    int ChargePercent,
    ChargeReason ChargeReason)
{
    public static AgendaItemResponse From(AgendaEntry e, ChargeDecision charge) => new(
        e.Appointment.Id, e.Appointment.PatientId, e.PatientDisplayName,
        e.Appointment.StartsAt, e.Appointment.EndsAt, e.Appointment.Status,
        charge.ChargePercent, charge.Reason);
}

public sealed record CancelAppointmentRequest(CancellationSource By, string Reason);

public sealed record RescheduleAppointmentRequest(DateTimeOffset StartsAt, int? DurationMinutes);
