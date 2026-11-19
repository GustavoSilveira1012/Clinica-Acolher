using ClinicaAcolher.Domain.Availability;

namespace ClinicaAcolher.Application.Availability;

public interface IAvailabilityRepository
{
    Task<ProfessionalAvailability?> GetByProfessionalAsync(Guid professionalId, CancellationToken ct);
    void Add(ProfessionalAvailability availability);

    Task<IReadOnlyList<AvailabilityBlock>> ListBlocksAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<AvailabilityBlock?> GetBlockAsync(Guid id, CancellationToken ct);
    void AddBlock(AvailabilityBlock block);
    void RemoveBlock(AvailabilityBlock block);
}