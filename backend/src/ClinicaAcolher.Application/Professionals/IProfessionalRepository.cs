using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Application.Professionals;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Professional>> ListAsync(CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    void Add(Professional professional);
}