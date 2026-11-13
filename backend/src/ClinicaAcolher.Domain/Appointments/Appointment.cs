using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Domain.Appointments;

public sealed class Appointment : Entity
{
    public static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(3);

    public Guid RelationshipId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StatusChangedAt { get; private set; }
    public CancellationSource? CancelledBy { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid? RescheduledToId { get; private set; }

    public TimeSpan Duration => EndsAt - StartsAt;

    // RN-CAN-06: com quanta antecedência foi cancelada (base para a política de faltas)
    public TimeSpan? CancellationNotice =>
        Status == AppointmentStatus.Cancelled && StatusChangedAt is { } cancelledAt
            ? StartsAt - cancelledAt
            : null;

    private Appointment() { }

    public static Appointment Schedule(
        TherapeuticRelationship relationship, DateTimeOffset startsAt, TimeSpan duration, DateTimeOffset now)
    {
        if (!relationship.AllowsScheduling)
            throw new DomainException("RN-VIN-04", "Só é possível agendar sessões para um vínculo ativo.");

        if (duration < MinDuration || duration > MaxDuration)
            throw new DomainException("RN-AGD-01", "A duração da sessão precisa ficar entre 15 minutos e 3 horas.");

        var start = startsAt.ToUniversalTime();

        return new Appointment
        {
            RelationshipId = relationship.Id,
            PatientId = relationship.PatientId,
            ProfessionalId = relationship.ProfessionalId,
            StartsAt = start,
            EndsAt = start + duration,
            CreatedAt = now,
        };
    }

    public void Confirm(DateTimeOffset now) => TransitionTo(AppointmentStatus.Confirmed, now);

    public void Complete(DateTimeOffset now)
    {
        EnsureStarted(now, "Uma sessão só pode ser registrada como realizada a partir do horário de início.");
        TransitionTo(AppointmentStatus.Completed, now);
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        EnsureStarted(now, "Uma falta só pode ser registrada a partir do horário de início da sessão.");
        TransitionTo(AppointmentStatus.NoShow, now);
    }

    public void Cancel(CancellationSource by, string reason, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("RN-CAN-06", "Informe o motivo do cancelamento.");

        TransitionTo(AppointmentStatus.Cancelled, now);
        CancelledBy = by;
        CancellationReason = reason.Trim();
    }

    // RN-CAN-03: a remarcação encerra esta sessão e aponta para a nova
    public void MarkRescheduled(Guid newAppointmentId, DateTimeOffset now)
    {
        TransitionTo(AppointmentStatus.Rescheduled, now);
        RescheduledToId = newAppointmentId;
    }

    private void EnsureStarted(DateTimeOffset now, string message)
    {
        if (now < StartsAt)
            throw new DomainException("RN-AGD-11", message);
    }

    private void TransitionTo(AppointmentStatus next, DateTimeOffset now)
    {
        if (!AppointmentStateMachine.CanTransition(Status, next))
            throw new DomainException("RN-AGD-08",
                $"Uma sessão {Describe(Status)} não pode passar para {Describe(next)}.");

        Status = next;
        StatusChangedAt = now;
    }

    private static string Describe(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Scheduled => "agendada",
        AppointmentStatus.Confirmed => "confirmada",
        AppointmentStatus.Completed => "realizada",
        AppointmentStatus.NoShow => "falta",
        AppointmentStatus.Cancelled => "cancelada",
        AppointmentStatus.Rescheduled => "remarcada",
        _ => status.ToString(),
    };
}