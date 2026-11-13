using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Attendance;

namespace ClinicaAcolher.Application.Attendance;

public sealed class AttendancePolicyService(
    IProfessionalRepository professionals,
    IPatientRepository patients,
    IAttendancePolicyRepository policies,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<AttendancePolicyResponse> PublishAsync(
        Guid professionalId, PublishAttendancePolicyRequest r, CancellationToken ct)
    {
        var professional = await professionals.GetByIdAsync(professionalId, ct)
            ?? throw new NotFoundException("Profissional não encontrado.");

        var current = await policies.GetLatestAsync(professionalId, ct);
        var nextVersion = (current?.Version ?? 0) + 1;

        var policy = AttendancePolicy.Publish(professional, nextVersion,
            r.MinimumNoticeHours, r.LateCancellationChargePercent, r.NoShowChargePercent, clock.GetUtcNow());

        policies.Add(policy);
        await unitOfWork.SaveChangesAsync(ct);

        return AttendancePolicyResponse.From(policy);
    }

    public async Task<AttendancePolicyResponse> GetCurrentAsync(Guid professionalId, CancellationToken ct) =>
        AttendancePolicyResponse.From(await policies.GetLatestAsync(professionalId, ct)
            ?? throw new NotFoundException("Este profissional ainda não publicou uma política de faltas."));

    public async Task<PolicyAcceptanceResponse> AcceptAsync(Guid policyId, AcceptPolicyRequest r, CancellationToken ct)
    {
        var policy = await policies.GetByIdAsync(policyId, ct)
            ?? throw new NotFoundException("Política não encontrada.");
        var patient = await patients.GetByIdAsync(r.PatientId, ct)
            ?? throw new NotFoundException("Paciente não encontrado.");

        var existing = await policies.FindAcceptanceAsync(patient.Id, policy.Id, ct);
        if (existing is not null)
            return PolicyAcceptanceResponse.From(existing);

        var acceptance = AttendancePolicyAcceptance.Record(patient, policy, clock.GetUtcNow());
        policies.AddAcceptance(acceptance);
        await unitOfWork.SaveChangesAsync(ct);

        return PolicyAcceptanceResponse.From(acceptance);
    }
}