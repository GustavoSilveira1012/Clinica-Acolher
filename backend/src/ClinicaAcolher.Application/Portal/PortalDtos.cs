using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Portal;

/// <summary>O que o paciente vê de uma sessão: só o necessário (RN-GER-03).</summary>
public sealed record PortalAppointment(
    Guid Id, DateTimeOffset StartsAt, DateTimeOffset EndsAt, AppointmentStatus Status, string ProfessionalName);

public sealed record PortalAgendaResponse(IReadOnlyList<PortalAppointment> Upcoming, IReadOnlyList<PortalAppointment> Recent);

public sealed record PortalPolicyResponse(
    Guid PolicyId,
    string ProfessionalName,
    int Version,
    int MinimumNoticeHours,
    int LateCancellationChargePercent,
    int NoShowChargePercent,
    DateTimeOffset PublishedAt,
    bool Accepted,
    DateTimeOffset? AcceptedAt);