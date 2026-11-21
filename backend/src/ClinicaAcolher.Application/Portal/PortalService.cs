using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Attendance;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Application.Portal;

public sealed class PortalService(
    IAppointmentRepository appointments,
    IRelationshipRepository relationships,
    IAttendancePolicyRepository policies,
    IProfessionalRepository professionals,
    IPatientRepository patients,
    IAuditTrail audit,
    AccessControl access,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<PortalAgendaResponse> GetAgendaAsync(CancellationToken ct)
    {
        var patientId = access.RequirePatient();
        var now = clock.GetUtcNow();

        var list = await appointments.ListForPatientAsync(patientId, now.AddDays(-90), now.AddDays(180), ct);

        // Uma sessão em andamento ainda conta como "próxima": é dela que o paciente precisa agora
        return new PortalAgendaResponse(
            list.Where(a => a.EndsAt > now).OrderBy(a => a.StartsAt).ToList(),
            list.Where(a => a.EndsAt <= now).OrderByDescending(a => a.StartsAt).ToList());
    }

    public async Task<IReadOnlyList<PortalPolicyResponse>> ListPoliciesAsync(CancellationToken ct)
    {
        var patientId = access.RequirePatient();
        var result = new List<PortalPolicyResponse>();

        foreach (var relationship in await relationships.ListByPatientAsync(patientId, ct))
        {
            if (relationship.Status == RelationshipStatus.Ended)
                continue;

            var policy = await policies.GetLatestAsync(relationship.ProfessionalId, ct);
            if (policy is null)
                continue;

            var acceptance = await policies.FindAcceptanceAsync(patientId, policy.Id, ct);
            result.Add(await ToResponseAsync(policy, acceptance, ct));
        }

        return result;
    }

    // RN-CAN-01: o próprio paciente aceita a versão vigente da política
    public async Task<PortalPolicyResponse> AcceptPolicyAsync(Guid policyId, CancellationToken ct)
    {
        var patientId = access.RequirePatient();

        var policy = await policies.GetByIdAsync(policyId, ct);
        if (policy is null || !await relationships.ExistsAsync(policy.ProfessionalId, patientId, ct))
            throw new NotFoundException("Política não encontrada.");

        // RN-CAN-02: só a versão mais nova pode ser aceita
        var latest = await policies.GetLatestAsync(policy.ProfessionalId, ct);
        if (latest!.Id != policy.Id)
            throw new DomainException("RN-CAN-02", "Existe uma versão mais nova desta política. Recarregue a página para lê-la.");

        var acceptance = await policies.FindAcceptanceAsync(patientId, policyId, ct);
        if (acceptance is null)
        {
            var patient = await patients.GetByIdAsync(patientId, ct)
                ?? throw new NotFoundException("Paciente não encontrado.");

            acceptance = AttendancePolicyAcceptance.Record(patient, policy, clock.GetUtcNow());
            policies.AddAcceptance(acceptance);
            audit.Record(new AuditEvent("AttendancePolicy.Accepted", "AttendancePolicy", policy.Id, patientId));
            await unitOfWork.SaveChangesAsync(ct);
        }

        return await ToResponseAsync(policy, acceptance, ct);
    }

    private async Task<PortalPolicyResponse> ToResponseAsync(
        AttendancePolicy policy, AttendancePolicyAcceptance? acceptance, CancellationToken ct)
    {
        var professional = await professionals.GetByIdAsync(policy.ProfessionalId, ct);

        return new PortalPolicyResponse(
            policy.Id, professional?.ProfessionalName ?? "Profissional", policy.Version,
            policy.MinimumNoticeHours, policy.LateCancellationChargePercent, policy.NoShowChargePercent,
            policy.PublishedAt, acceptance is not null, acceptance?.AcceptedAt);
    }
}