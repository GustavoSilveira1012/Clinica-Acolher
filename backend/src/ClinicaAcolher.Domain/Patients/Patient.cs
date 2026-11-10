using ClinicaAcolher.Domain.Common;

namespace ClinicaAcolher.Domain.Patients;

public sealed class Patient : Entity
{
    public const int NameMaxLength = 160;

    public string FullName {get; private set;} = string.Empty;
    public string? SocialName {get; private set;}
    public string? Email {get; private set;}
    public string? Phone {get; private set;}
    public DateOnly BirthDate {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public bool Active { get; private set; } = true;
    public DateTimeOffset? DeactivatedAt { get; private set; }
    public DateTimeOffset? ReactivatedAt { get; private set; }
    public string? DeactivationReason { get; private set; }

    // RN-PAC-07: o nome social, quando existe, substitui o nome civil nas telas.
    public string DisplayName => SocialName ?? FullName;

    private Patient() {}

    public static Patient Create(string FullName, DateOnly birthDate, DateTimeOffset now,
        string? socialName = null, string? email = null, string? phone = null)
    {
        var patient = new Patient {CreatedAt = now};
        patient.UpdateData(FullName, birthDate, now, socialName, email, phone);
        return patient;
    }

    public void UpdateData(string fullName, DateOnly birthDate, DateTimeOffset now,
        string? socialName, string? email, string? phone)
    {
        FullName = ValidateName(fullName);
        SocialName = string.IsNullOrWhiteSpace(socialName) ? null : ValidateName(socialName);
        BirthDate = ValidateBirthDate(birthDate, now);
        Email = NormalizeEmail(email);
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    // RN-PAC-02: paciente nunca é apagado, só desativado.
    public void Deactivate(string reason, DateTimeOffset now)
    {
        if (!Active)
        throw new DomainException("RN-PAC-02", "O pacienete ja está desaticado.");
        if (string.IsNullOrWhiteSpace(reason))
        throw new DomainException("RN-PAC-02", "Informe o motivo da desativação.");

        Active = false;
        DeactivatedAt = now;
        DeactivationReason = reason.Trim();
    }

    public void Reactivate(DateTimeOffset now)
    {
        if (Active)
        throw new DomainException("RN-PAC-02", "O paciente já está ativo.");

        Active = true;
        ReactivatedAt = now;
    }

    // Base para RN-PAC-03: menores só são atendidos com reponsável legal.
    public bool IsMinor(DateOnly today) => BirthDate.AddYears(18) > today;

    private static string ValidateName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length < 2 || trimmed.Length > NameMaxLength)
            throw new DomainException("RN-PAC-01", $"O nome precisa ter entre 2 e {NameMaxLength} caracteres.");
        return trimmed;
    }

    private static DateOnly ValidateBirthDate(DateOnly birthDate, DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (birthDate > today || birthDate.Year < 1900)
            throw new DomainException("RN-PAC-01", "Informe uma data de nascimento válida.");
        return birthDate;
    }

    private static string? NormalizeEmail(string? email)
    {
        if ( string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        if (!normalized.Contains('@') || normalized.Contains(' '))
            throw new DomainException("RN-PAC-01", "Informe um email válido.");
        return normalized;
    }
}
