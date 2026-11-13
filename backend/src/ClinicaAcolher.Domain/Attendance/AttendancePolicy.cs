using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Domain.Attendance;

public sealed class AttendancePolicy : Entity
{
    public const int MaxNoticeHours = 168;

    public Guid ProfessionalId { get; private set; }
    public int Version { get; private set; }
    public int MinimumNoticeHours { get; private set; }
    public int LateCancellationChargePercent { get; private set; }
    public int NoShowChargePercent { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }

    public TimeSpan MinimumNotice => TimeSpan.FromHours(MinimumNoticeHours);

    private AttendancePolicy() { }

    public static AttendancePolicy Publish(Professional professional, int version, int minimumNoticeHours,
        int lateCancellationChargePercent, int noShowChargePercent, DateTimeOffset now)
    {
        if (version < 1)
            throw new DomainException("RN-CAN-01", "A versão da política precisa ser 1 ou maior.");

        if (minimumNoticeHours < 0 || minimumNoticeHours > MaxNoticeHours)
            throw new DomainException("RN-CAN-01", $"A antecedência mínima precisa ficar entre 0 e {MaxNoticeHours} horas.");

        EnsurePercent(lateCancellationChargePercent);
        EnsurePercent(noShowChargePercent);

        return new AttendancePolicy
        {
            ProfessionalId = professional.Id,
            Version = version,
            MinimumNoticeHours = minimumNoticeHours,
            LateCancellationChargePercent = lateCancellationChargePercent,
            NoShowChargePercent = noShowChargePercent,
            PublishedAt = now,
        };
    }

    public ChargeDecision Evaluate(Appointment appointment)
    {
        if (appointment.ProfessionalId != ProfessionalId)
            throw new DomainException("RN-CAN-01", "Esta política não pertence ao profissional da sessão.");

        return appointment.Status switch
        {
            AppointmentStatus.NoShow =>
                ChargeDecision.Of(NoShowChargePercent, ChargeReason.NoShow),

            AppointmentStatus.Cancelled
                when appointment.CancelledBy == CancellationSource.Patient
                  && appointment.CancellationNotice < MinimumNotice =>
                ChargeDecision.Of(LateCancellationChargePercent, ChargeReason.LateCancellation),

            _ => ChargeDecision.NoCharge,
        };
    }

    private static void EnsurePercent(int percent)
    {
        if (percent < 0 || percent > 100)
            throw new DomainException("RN-CAN-01", "O percentual de cobrança precisa ficar entre 0 e 100.");
    }
}