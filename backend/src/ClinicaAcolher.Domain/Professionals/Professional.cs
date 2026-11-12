using System.Net.Http.Headers;
using ClinicaAcolher.Domain.Common;

namespace ClinicaAcolher.Domain.Professionals;

public sealed class Professional : Entity
{
    public const int NameMaxLength = 160;

    public string FullName {get; private set;} = string.Empty;
    public string ProfessionalName {get; private set;} = string.Empty;
    public string Email {get; private set;} = string.Empty;
    public ProfessionalCategory Category {get; private set;}
    public string? RegistrationNumber {get; private set;}
    public VerificationStatus Verification { get; private set; } = VerificationStatus.Pending;
    public DateTimeOffset? VerifiedAt {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}

    // RN-PRO-04: só atende depois de aprovado
    public bool CanAttend => Verification == VerificationStatus.Approved;

    private Professional() {}

    public static Professional Create(string fullName, string professionalName, string email,
    ProfessionalCategory category, string? registrationNumber, DateTimeOffset now)
    {
        var registration = string.IsNullOrWhiteSpace(registrationNumber) ? null : registrationNumber.Trim();

        if (category == ProfessionalCategory.Psychologist && registration is null)
            throw new DomainException("RN-PRO-02", "Psicólogos precisam informar o numero de registro no CRP.");

        return new Professional
        {
            FullName = ValidateName(fullName),
            ProfessionalName = ValidateName(professionalName),
            Email = ValidateEmail(email),
            Category = category,
            RegistrationNumber = registration,
            CreatedAt = now,
        };
    }

    public void Approve(DateTimeOffset now)
    {
        if (Verification == VerificationStatus.Approved)
            throw new DomainException("RN-PRO-04", "O profissional ja esta aprovado.");

        Verification = VerificationStatus.Approved;
        VerifiedAt = now;
    }

    public void Reject(DateTimeOffset now)
    {
        if (Verification != VerificationStatus.Pending)
            throw new DomainException("RN-PRO-04", "Só um cadastro pendente pode ser recusado.");

        Verification = VerificationStatus.Rejected;
        VerifiedAt = now;
    }

    private static string ValidateName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if(trimmed.Length < 2 || trimmed.Length > NameMaxLength)
            throw new DomainException("RN-PRO-01", $"O nome precisa ter entre 2 e {NameMaxLength} caracteres.");
        return trimmed;
    }

    private static string ValidateEmail(string email)
    {
        var normalized = email?.Trim().ToLowerInvariant() ?? string.Empty;
        if(!normalized.Contains("@") || normalized.Contains(' '))
            throw new DomainException("RN-PRO-01", "Informe um e-mail válido.");
        return normalized;
    }
}
