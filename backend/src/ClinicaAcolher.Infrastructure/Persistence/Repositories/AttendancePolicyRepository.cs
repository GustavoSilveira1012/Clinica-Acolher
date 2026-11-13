using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Domain.Attendance;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class AttendancePolicyRepository(AppDbContext db) : IAttendancePolicyRepository
{
    public Task<AttendancePolicy?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AttendancePolicies.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<AttendancePolicy?> GetLatestAsync(Guid professionalId, CancellationToken ct) =>
        db.AttendancePolicies
            .Where(p => p.ProfessionalId == professionalId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<AttendancePolicy>> ListByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct) =>
        await db.AttendancePolicies.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public Task<AttendancePolicyAcceptance?> FindAcceptanceAsync(Guid patientId, Guid policyId, CancellationToken ct) =>
        db.AttendancePolicyAcceptances.FirstOrDefaultAsync(a => a.PatientId == patientId && a.PolicyId == policyId, ct);

    public async Task<IReadOnlyList<AttendancePolicyAcceptance>> ListAcceptancesAsync(
        Guid professionalId, IReadOnlyList<Guid> patientIds, CancellationToken ct) =>
        await db.AttendancePolicyAcceptances
            .AsNoTracking()
            .Where(a => a.ProfessionalId == professionalId && patientIds.Contains(a.PatientId))
            .ToListAsync(ct);

    public void Add(AttendancePolicy policy) => db.AttendancePolicies.Add(policy);

    public void AddAcceptance(AttendancePolicyAcceptance acceptance) => db.AttendancePolicyAcceptances.Add(acceptance);
}