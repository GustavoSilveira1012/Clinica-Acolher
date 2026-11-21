using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.Domain.PatientAccess;

// RN-POR-01: convite de uso único, válido por 72 horas
public sealed class PatientInvitation : Entity
{
    public static readonly TimeSpan Validity = TimeSpan.FromHours(72);

    public Guid PatientId { get; private set; }
    public byte[] TokenHash { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private PatientInvitation() { }

    // RN-POR-02: paciente ativo, maior de idade e com e-mail
    public static PatientInvitation Issue(Patient patient, byte[] tokenHash, DateTimeOffset now)
    {
        if (!patient.Active)
            throw new DomainException("RN-POR-02", "Paciente desativado não recebe convite para o portal.");

        if (patient.IsMinor(DateOnly.FromDateTime(now.UtcDateTime)))
            throw new DomainException("RN-POR-02", "O acesso de menores ao portal será feito pelo responsável legal, e ainda não está disponível.");

        if (string.IsNullOrWhiteSpace(patient.Email))
            throw new DomainException("RN-POR-02", "Cadastre o e-mail do paciente antes de convidá-lo para o portal.");

        return new PatientInvitation
        {
            PatientId = patient.Id,
            TokenHash = tokenHash.ToArray(),
            CreatedAt = now,
            ExpiresAt = now + Validity,
        };
    }

    public bool IsUsable(DateTimeOffset now) => UsedAt is null && RevokedAt is null && now < ExpiresAt;

    public void Revoke(DateTimeOffset now)
    {
        if (UsedAt is null && RevokedAt is null)
            RevokedAt = now;
    }

    public void Accept(DateTimeOffset now)
    {
        if (!IsUsable(now))
            throw new DomainException("RN-POR-01", "Este convite é inválido ou expirou.");

        UsedAt = now;
    }
}