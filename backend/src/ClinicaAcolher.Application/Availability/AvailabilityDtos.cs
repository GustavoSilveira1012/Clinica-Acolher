using ClinicaAcolher.Domain.Availability;

namespace ClinicaAcolher.Application.Availability;

public sealed record AvailabilityWindowDto(DayOfWeek Weekday, TimeOnly Start, TimeOnly End);

public sealed record UpdateAvailabilityRequest(
    string TimeZoneId, int DefaultSessionMinutes, int BreakMinutes, IReadOnlyList<AvailabilityWindowDto> Windows);

public sealed record AvailabilityResponse(
    Guid ProfessionalId,
    string TimeZoneId,
    int DefaultSessionMinutes,
    int BreakMinutes,
    IReadOnlyList<AvailabilityWindowDto> Windows,
    DateTimeOffset UpdatedAt)
{
    public static AvailabilityResponse From(ProfessionalAvailability a) => new(
        a.ProfessionalId, a.TimeZoneId, a.DefaultSessionMinutes, a.BreakMinutes,
        a.Windows.Select(w => new AvailabilityWindowDto(w.Weekday, w.Start, w.End)).ToList(),
        a.UpdatedAt);
}

/// <summary>Configured = false quando a profissional ainda não definiu horários: nesse caso, não há o que avisar.</summary>
public sealed record AvailabilityCheckResponse(bool Configured, bool WithinAvailability, bool Blocked);

public sealed record CreateBlockRequest(DateTimeOffset StartsAt, DateTimeOffset EndsAt, string? Reason);

public sealed record BlockResponse(Guid Id, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string? Reason)
{
    public static BlockResponse From(AvailabilityBlock b) => new(b.Id, b.StartsAt, b.EndsAt, b.Reason);
}

/// <summary>RN-AGD-14: ao criar um bloqueio, a profissional fica sabendo quantas sessões caem no período.</summary>
public sealed record BlockCreatedResponse(BlockResponse Block, int AffectedAppointments);