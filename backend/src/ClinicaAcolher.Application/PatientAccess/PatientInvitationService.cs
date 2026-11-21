using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.PatientAccess;

namespace ClinicaAcolher.Application.PatientAccess;

public sealed class PatientInvitationService(
    IPatientRepository patients,
    IPatientInvitationRepository invitations,
    IPatientAccounts accounts,
    IAuditTrail audit,
    AccessControl access,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    private const string InvalidInvitation =
        "Este convite é inválido ou expirou. Peça um novo convite ao seu profissional.";

    public async Task<InvitationIssuedResponse> IssueAsync(Guid patientId, CancellationToken ct)
    {
        await access.EnsureCanAccessPatientAsync(patientId, ct);
        var patient = await patients.GetByIdAsync(patientId, ct)
            ?? throw new NotFoundException("Paciente não encontrado.");

        if (await accounts.HasAccountAsync(patientId, ct))
            throw new ConflictException("RN-POR-01", "Este paciente já tem acesso ao portal.");

        var now = clock.GetUtcNow();

        // RN-POR-01: um convite novo invalida os anteriores
        foreach (var open in await invitations.ListOpenByPatientAsync(patientId, ct))
            open.Revoke(now);

        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var invitation = PatientInvitation.Issue(patient, Hash(token), now);

        invitations.Add(invitation);
        audit.Record(new AuditEvent("PatientInvitation.Issued", "PatientInvitation", invitation.Id, patientId));
        await unitOfWork.SaveChangesAsync(ct);

        // RN-POR-03: o código sai daqui uma única vez; o banco fica só com o hash
        return new InvitationIssuedResponse(token, invitation.ExpiresAt);
    }

    public async Task AcceptAsync(AcceptInvitationRequest r, CancellationToken ct)
    {
        var now = clock.GetUtcNow();

        var invitation = string.IsNullOrWhiteSpace(r.Token)
            ? null
            : await invitations.FindByTokenHashAsync(Hash(r.Token), ct);

        var patient = invitation is null ? null : await patients.GetByIdAsync(invitation.PatientId, ct);

        // A mesma resposta para código errado, usado, expirado ou de paciente desativado
        if (invitation is null || patient is null || !patient.Active || !invitation.IsUsable(now))
            throw new DomainException("RN-POR-01", InvalidInvitation);

        if (r.Password.Length < 10)
            throw new DomainException("RN-SEG-02", "A senha precisa ter pelo menos 10 caracteres.");

        var userId = Guid.Empty;
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            userId = await accounts.CreateAsync(patient.Id, patient.Email!, r.Password, ct);
            invitation.Accept(now);
            audit.Record(new AuditEvent("PatientInvitation.Accepted", "PatientInvitation", invitation.Id, patient.Id));
            await unitOfWork.SaveChangesAsync(ct);
        }, ct);

        await accounts.SignInAsync(userId);
    }

    private static byte[] Hash(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}