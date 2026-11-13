using ClinicaAcolher.Domain.Attendance;

namespace ClinicaAcolher.Application.Attendance;

public interface IAttendancePolicyRepository
{
    Task<AttendancePolicy?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<AttendancePolicy?> GetLatestAsync(Guid professionalId, CancellationToken ct);
    Task<IReadOnlyList<AttendancePolicy>> ListByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct);
    Task<AttendancePolicyAcceptance?> FindAcceptanceAsync(Guid patientId, Guid policyId, CancellationToken ct);
    Task<IReadOnlyList<AttendancePolicyAcceptance>> ListAcceptancesAsync(
        Guid professionalId, IReadOnlyList<Guid> patientIds, CancellationToken ct);
    void Add(AttendancePolicy policy);
    void AddAcceptance(AttendancePolicyAcceptance acceptance);
}