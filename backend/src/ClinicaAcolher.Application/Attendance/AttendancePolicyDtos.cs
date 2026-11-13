using ClinicaAcolher.Domain.Attendance;

namespace ClinicaAcolher.Application.Attendance;

public sealed record PublishAttendancePolicyRequest(
    int MinimumNoticeHours, int LateCancellationChargePercent, int NoShowChargePercent);

public sealed record AttendancePolicyResponse(
    Guid Id,
    Guid ProfessionalId,
    int Version,
    int MinimumNoticeHours,
    int LateCancellationChargePercent,
    int NoShowChargePercent,
    DateTimeOffset PublishedAt)
{
    public static AttendancePolicyResponse From(AttendancePolicy p) => new(
        p.Id, p.ProfessionalId, p.Version, p.MinimumNoticeHours,
        p.LateCancellationChargePercent, p.NoShowChargePercent, p.PublishedAt);
}

public sealed record AcceptPolicyRequest(Guid PatientId);

public sealed record PolicyAcceptanceResponse(
    Guid Id, Guid PatientId, Guid PolicyId, int PolicyVersion, DateTimeOffset AcceptedAt)
{
    public static PolicyAcceptanceResponse From(AttendancePolicyAcceptance a) => new(
        a.Id, a.PatientId, a.PolicyId, a.PolicyVersion, a.AcceptedAt);
}