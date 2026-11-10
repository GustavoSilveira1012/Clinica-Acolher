using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.Application.Patients;

public sealed record CreatePatientRequest(
    string FullName,
    DateOnly BirthDate,
    string? SocialName,
    string? Email,
    string? Phone
);

public sealed record PatientResponse(
    Guid Id,
    string DisplayName,
    string FullName,
    string? SocialName,
    string? Email,
    string? Phone,
    DateOnly BirthDate,
    bool Active,
    DateTimeOffset CreatedAt
)
{
    public static PatientResponse From(Patient p) => new(p.Id, p.DisplayName, p.FullName, p.SocialName, p.Email,
        p.Phone, p.BirthDate, p.Active, p.CreatedAt);
}