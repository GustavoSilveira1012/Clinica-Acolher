using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Application.Professionals;

public sealed class ProfessionalService(
    IProfessionalRepository professionals, IUnitOfWork unitOfWork, TimeProvider clock)
{
    public async Task<ProfessionalResponse> CreateAsync(CreateProfessionalRequest r, CancellationToken ct)
    {
        var professional = Professional.Create(
            r.FullName, r.ProfessionalName, r.Email, r.Category, r.RegistrationNumber, clock.GetUtcNow());

        if (await professionals.EmailExistsAsync(professional.Email, ct))
            throw new DomainException("RN-PRO-01", "Já existe um profissional cadastrado com este e-mail.");

        professionals.Add(professional);
        await unitOfWork.SaveChangesAsync(ct);

        return ProfessionalResponse.From(professional);
    }

    public async Task<ProfessionalResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var professional = await professionals.GetByIdAsync(id, ct);
        return professional is null ? null : ProfessionalResponse.From(professional);
    }

    public Task<ProfessionalResponse?> ApproveAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (p, now) => p.Approve(now), ct);

    public Task<ProfessionalResponse?> RejectAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (p, now) => p.Reject(now), ct);

    private async Task<ProfessionalResponse?> ChangeAsync(
        Guid id, Action<Professional, DateTimeOffset> change, CancellationToken ct)
    {
        var professional = await professionals.GetByIdAsync(id, ct);
        if (professional is null)
            return null;

        change(professional, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);

        return ProfessionalResponse.From(professional);
    }
}