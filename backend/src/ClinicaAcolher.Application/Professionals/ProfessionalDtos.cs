using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Application.Professionals;

public sealed record CreateProfessionalRequest(
    string FullName,
    string ProfessionalName,
    string Email,
    ProfessionalCategory Category,
    String? RegistrationNumber
);

public sealed record ProfessionalResponse(
    Guid Id,
    string FullName,
    string ProfessionalName,
    string Email,
    ProfessionalCategory Category,
    string? RegistrationNumber,
    VerificationStatus Verification,
    bool CanAttend,
    DateTimeOffset CreatedAt
)
{
    public static ProfessionalResponse From(Professional p) => new(
        p.Id, p.FullName, p.ProfessionalName, p.Email, p.Category,
        p.RegistrationNumber, p.Verification, p.CanAttend, p.CreatedAt
    );
}