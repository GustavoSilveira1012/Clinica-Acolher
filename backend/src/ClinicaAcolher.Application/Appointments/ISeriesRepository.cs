using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public interface ISeriesRepository
{
    Task<AppointmentSeries?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Guid>> ListDueIdsAsync(DateOnly until, CancellationToken ct);
    void Add(AppointmentSeries series);
}