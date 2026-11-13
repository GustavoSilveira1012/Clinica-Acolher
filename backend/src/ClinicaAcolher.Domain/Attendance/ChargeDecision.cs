namespace ClinicaAcolher.Domain.Attendance;

public sealed record ChargeDecision(int ChargePercent, ChargeReason Reason)
{
    public static readonly ChargeDecision NoCharge = new(0, ChargeReason.None);

    public bool IsChargeable => ChargePercent > 0;

    public static ChargeDecision Of(int percent, ChargeReason reason) =>
        percent > 0 ? new ChargeDecision(percent, reason) : NoCharge;
}