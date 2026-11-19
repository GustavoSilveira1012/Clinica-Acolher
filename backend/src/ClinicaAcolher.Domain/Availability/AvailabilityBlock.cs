using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Domain.Availability;

// RN-AGD-02: férias, feriados e congressos suspendem a disponibilidade sem apagar as séries
public sealed class AvailabilityBlock : Entity
{
    public const int MaxReasonLength = 120;
    public static readonly TimeSpan MaxDuration = TimeSpan.FromDays(366);

    public Guid ProfessionalId { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AvailabilityBlock() { }

    public static AvailabilityBlock Create(Professional professional, DateTimeOffset startsAt, DateTimeOffset endsAt,
        string? reason, DateTimeOffset now)
    {
        if (endsAt <= startsAt)
            throw new DomainException("RN-AGD-02", "O fim do bloqueio precisa ser depois do início.");

        if (endsAt - startsAt > MaxDuration)
            throw new DomainException("RN-AGD-02", "Um bloqueio pode durar no máximo um ano.");

        var trimmed = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (trimmed?.Length > MaxReasonLength)
            throw new DomainException("RN-AGD-02", $"O motivo pode ter no máximo {MaxReasonLength} caracteres.");

        return new AvailabilityBlock
        {
            ProfessionalId = professional.Id,
            StartsAt = startsAt.ToUniversalTime(),
            EndsAt = endsAt.ToUniversalTime(),
            Reason = trimmed,
            CreatedAt = now,
        };
    }

    public bool Overlaps(DateTimeOffset start, DateTimeOffset end) => StartsAt < end && start < EndsAt;
}