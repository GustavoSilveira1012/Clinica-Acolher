using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Availability;

namespace ClinicaAcolher.Application.Availability;

public sealed class AvailabilityService(
    IProfessionalRepository professionals,
    IAvailabilityRepository availabilities,
    IAppointmentRepository appointments,
    AccessControl access,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<AvailabilityResponse> GetAsync(Guid professionalId, CancellationToken ct)
    {
        access.EnsureActsFor(professionalId, "Profissional não encontrado.");

        var availability = await availabilities.GetByProfessionalAsync(professionalId, ct)
            ?? throw new NotFoundException("Este profissional ainda não configurou a disponibilidade.");
        return AvailabilityResponse.From(availability);
    }

    public async Task<AvailabilityResponse> SaveAsync(Guid professionalId, UpdateAvailabilityRequest r, CancellationToken ct)
    {
        access.EnsureActsFor(professionalId, "Profissional não encontrado.");

        var professional = await professionals.GetByIdAsync(professionalId, ct)
            ?? throw new NotFoundException("Profissional não encontrado.");

        var windows = r.Windows.Select(w => new AvailabilityWindow(w.Weekday, w.Start, w.End));
        var now = clock.GetUtcNow();

        var availability = await availabilities.GetByProfessionalAsync(professionalId, ct);
        if (availability is null)
        {
            availability = ProfessionalAvailability.Create(
                professional, r.TimeZoneId, r.DefaultSessionMinutes, r.BreakMinutes, windows, now);
            availabilities.Add(availability);
        }
        else
        {
            availability.Update(r.TimeZoneId, r.DefaultSessionMinutes, r.BreakMinutes, windows, now);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return AvailabilityResponse.From(availability);
    }

    // RN-AGD-13: a tela pergunta antes de salvar, para avisar a profissional
    public async Task<AvailabilityCheckResponse> CheckAsync(
        Guid professionalId, DateTimeOffset startsAt, int durationMinutes, CancellationToken ct)
    {
        access.EnsureActsFor(professionalId, "Profissional não encontrado.");

        var duration = TimeSpan.FromMinutes(durationMinutes);
        var endsAt = startsAt + duration;

        var availability = await availabilities.GetByProfessionalAsync(professionalId, ct);
        var blocks = await availabilities.ListBlocksAsync(professionalId, startsAt, endsAt, ct);

        return new AvailabilityCheckResponse(
            Configured: availability is not null,
            WithinAvailability: availability?.Covers(startsAt, duration) ?? false,
            Blocked: blocks.Count > 0);
    }

    public async Task<IReadOnlyList<BlockResponse>> ListBlocksAsync(
        Guid professionalId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        access.EnsureActsFor(professionalId, "Profissional não encontrado.");

        var blocks = await availabilities.ListBlocksAsync(
            professionalId, from.ToUniversalTime(), to.ToUniversalTime(), ct);
        return blocks.Select(BlockResponse.From).ToList();
    }

    public async Task<BlockCreatedResponse> CreateBlockAsync(Guid professionalId, CreateBlockRequest r, CancellationToken ct)
    {
        access.EnsureActsFor(professionalId, "Profissional não encontrado.");

        var professional = await professionals.GetByIdAsync(professionalId, ct)
            ?? throw new NotFoundException("Profissional não encontrado.");

        var block = AvailabilityBlock.Create(professional, r.StartsAt, r.EndsAt, r.Reason, clock.GetUtcNow());
        availabilities.AddBlock(block);
        await unitOfWork.SaveChangesAsync(ct);

        // RN-AGD-14: as sessões continuam marcadas; a profissional decide o que fazer com cada uma
        var affected = await appointments.CountActiveInPeriodAsync(professionalId, block.StartsAt, block.EndsAt, ct);
        return new BlockCreatedResponse(BlockResponse.From(block), affected);
    }

    public async Task DeleteBlockAsync(Guid blockId, CancellationToken ct)
    {
        var block = await availabilities.GetBlockAsync(blockId, ct)
            ?? throw new NotFoundException("Bloqueio não encontrado.");
        access.EnsureActsFor(block.ProfessionalId, "Bloqueio não encontrado.");

        availabilities.RemoveBlock(block);
        await unitOfWork.SaveChangesAsync(ct);
    }
}