using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<AgendaItemResponse>> ListAgendaAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    void Add(Appointment appointment);
}