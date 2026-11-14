using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Domain.Appointments;

public sealed class AppointmentSeries : Entity
{
    public Guid RelationshipId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DayOfWeek Weekday { get; private set; }
    public TimeOnly LocalStartTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public string TimeZoneId { get; private set; } = string.Empty;
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public DateOnly? GeneratedUntil { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsActive => EndsOn is null;
    public TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);

    private AppointmentSeries() { }

    public static AppointmentSeries Create(TherapeuticRelationship relationship, DayOfWeek weekday,
        TimeOnly localStartTime, int durationMinutes, string timeZoneId, DateOnly startsOn, DateTimeOffset now)
    {
        if (!relationship.AllowsScheduling)
            throw new DomainException("RN-VIN-04", "Só é possível criar séries para um vínculo ativo.");

        var duration = TimeSpan.FromMinutes(durationMinutes);
        if (duration < Appointment.MinDuration || duration > Appointment.MaxDuration)
            throw new DomainException("RN-AGD-01", "A duração da sessão precisa ficar entre 15 minutos e 3 horas.");

        FindTimeZone(timeZoneId); // valida o fuso agora, para não descobrir o erro só na geração

        return new AppointmentSeries
        {
            RelationshipId = relationship.Id,
            PatientId = relationship.PatientId,
            ProfessionalId = relationship.ProfessionalId,
            Weekday = weekday,
            LocalStartTime = localStartTime,
            DurationMinutes = durationMinutes,
            TimeZoneId = timeZoneId,
            StartsOn = startsOn,
            CreatedAt = now,
        };
    }

    // RN-AGD-12: cada ocorrência mantém a hora local e é convertida para UTC na própria data
    public IEnumerable<DateTimeOffset> OccurrencesBetween(DateOnly from, DateOnly until)
    {
        var timeZone = FindTimeZone(TimeZoneId);
        var first = from > StartsOn ? from : StartsOn;
        var last = EndsOn is { } end && end < until ? end : until;

        var daysUntilWeekday = ((int)Weekday - (int)first.DayOfWeek + 7) % 7;

        for (var date = first.AddDays(daysUntilWeekday); date <= last; date = date.AddDays(7))
        {
            var local = date.ToDateTime(LocalStartTime);

            // Na noite em que um horário de verão começa, uma hora "não existe" no relógio local
            if (timeZone.IsInvalidTime(local))
                local = local.AddHours(1);

            yield return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timeZone), TimeSpan.Zero);
        }
    }

    // RN-AGD-05: gera as sessões até uma data, sem repetir as que já foram geradas
    public IReadOnlyList<Appointment> GenerateUntil(TherapeuticRelationship relationship, DateOnly until, DateTimeOffset now)
    {
        if (relationship.Id != RelationshipId)
            throw new DomainException("RN-AGD-05", "Este vínculo não pertence à série.");

        if (!relationship.AllowsScheduling)
            return [];

        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var from = GeneratedUntil is { } generated ? generated.AddDays(1) : StartsOn;
        if (from < today)
            from = today;

        if (from > until)
            return [];

        var appointments = OccurrencesBetween(from, until)
            .Select(start => Appointment.Schedule(relationship, start, Duration, now, Id))
            .ToList();

        GeneratedUntil = until;
        return appointments;
    }

    public void End(DateOnly lastDay)
    {
        if (EndsOn is not null)
            throw new DomainException("RN-AGD-06", "Esta série já foi encerrada.");

        EndsOn = lastDay;
    }

    public bool IsAfterEnd(DateTimeOffset startsAt)
    {
        if (EndsOn is not { } end)
            return false;

        var local = TimeZoneInfo.ConvertTime(startsAt, FindTimeZone(TimeZoneId));
        return DateOnly.FromDateTime(local.DateTime) > end;
    }

    private static TimeZoneInfo FindTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new DomainException("RN-AGD-09", $"Fuso horário desconhecido: {timeZoneId}.");
        }
    }
}
