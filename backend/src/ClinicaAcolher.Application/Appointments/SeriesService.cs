using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.Application.Appointments;

public sealed class SeriesService(
    IRelationshipRepository relationships,
    ISeriesRepository series,
    IAppointmentRepository appointments,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    // RN-AGD-05: sempre as próximas 12 semanas geradas
    public const int WindowDays = 84;

    public async Task<SeriesResponse> CreateAsync(CreateSeriesRequest r, CancellationToken ct)
    {
        var relationship = await relationships.GetByIdAsync(r.RelationshipId, ct)
            ?? throw new NotFoundException("Vínculo não encontrado.");

        var now = clock.GetUtcNow();
        var created = AppointmentSeries.Create(relationship, r.Weekday, r.LocalStartTime,
            r.DurationMinutes, r.TimeZoneId, r.StartsOn, now);

        series.Add(created);
        foreach (var appointment in created.GenerateUntil(relationship, WindowEnd(), now))
            appointments.Add(appointment);

        await unitOfWork.SaveChangesAsync(ct);
        return SeriesResponse.From(created);
    }

    public async Task<SeriesResponse> GetByIdAsync(Guid id, CancellationToken ct) =>
        SeriesResponse.From(await LoadAsync(id, ct));

    public async Task<SeriesResponse> EndAsync(Guid id, EndSeriesRequest r, CancellationToken ct)
    {
        var existing = await LoadAsync(id, ct);
        var now = clock.GetUtcNow();
        existing.End(r.LastDay);

        var future = await appointments.ListActiveBySeriesAfterAsync(id, now, ct);
        foreach (var appointment in future.Where(a => existing.IsAfterEnd(a.StartsAt)))
            appointment.Cancel(CancellationSource.Professional, "Série encerrada", now);

        await unitOfWork.SaveChangesAsync(ct);
        return SeriesResponse.From(existing);
    }

    public Task<IReadOnlyList<Guid>> ListDueAsync(CancellationToken ct) =>
        series.ListDueIdsAsync(WindowEnd(), ct);

    public async Task<int> GenerateAsync(Guid id, CancellationToken ct)
    {
        var existing = await LoadAsync(id, ct);
        var relationship = await relationships.GetByIdAsync(existing.RelationshipId, ct)
            ?? throw new NotFoundException("Vínculo não encontrado.");

        var created = existing.GenerateUntil(relationship, WindowEnd(), clock.GetUtcNow());
        foreach (var appointment in created)
            appointments.Add(appointment);

        await unitOfWork.SaveChangesAsync(ct);
        return created.Count;
    }

    private DateOnly WindowEnd() =>
        DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime).AddDays(WindowDays);

    private async Task<AppointmentSeries> LoadAsync(Guid id, CancellationToken ct) =>
        await series.GetByIdAsync(id, ct) ?? throw new NotFoundException("Série não encontrada.");
}