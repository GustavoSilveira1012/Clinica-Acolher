using ClinicaAcolher.Application.Portal;
using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> ListActiveBySeriesAfterAsync(Guid seriesId, DateTimeOffset after, CancellationToken ct);
    Task<IReadOnlyList<AgendaEntry>> ListAgendaAsync(
    Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<int> CountActiveInPeriodAsync(Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IReadOnlyList<PortalAppointment>> ListForPatientAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    void Add(Appointment appointment);
}
