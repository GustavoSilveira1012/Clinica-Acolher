using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Domain.Availability;

public sealed class ProfessionalAvailability : Entity
{
    public const int MaxWindows = 21;
    public const int MaxBreakMinutes = 60;

    private readonly List<AvailabilityWindow> _windows = [];

    public Guid ProfessionalId { get; private set; }
    public string TimeZoneId { get; private set; } = string.Empty;
    public int DefaultSessionMinutes { get; private set; }
    public int BreakMinutes { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<AvailabilityWindow> Windows => _windows;
    public TimeSpan DefaultSession => TimeSpan.FromMinutes(DefaultSessionMinutes);

    private ProfessionalAvailability() { }

    public static ProfessionalAvailability Create(Professional professional, string timeZoneId,
        int defaultSessionMinutes, int breakMinutes, IEnumerable<AvailabilityWindow> windows, DateTimeOffset now)
    {
        var availability = new ProfessionalAvailability { ProfessionalId = professional.Id };
        availability.Update(timeZoneId, defaultSessionMinutes, breakMinutes, windows, now);
        return availability;
    }

    // RN-AGD-01: dias, horários, duração padrão e intervalo entre sessões
    public void Update(string timeZoneId, int defaultSessionMinutes, int breakMinutes,
        IEnumerable<AvailabilityWindow> windows, DateTimeOffset now)
    {
        TimeZones.Find(timeZoneId);

        var session = TimeSpan.FromMinutes(defaultSessionMinutes);
        if (session < Appointment.MinDuration || session > Appointment.MaxDuration)
            throw new DomainException("RN-AGD-01", "A duração padrão precisa ficar entre 15 minutos e 3 horas.");

        if (breakMinutes < 0 || breakMinutes > MaxBreakMinutes)
            throw new DomainException("RN-AGD-01", $"O intervalo entre sessões precisa ficar entre 0 e {MaxBreakMinutes} minutos.");

        var ordered = windows.OrderBy(w => w.Weekday).ThenBy(w => w.Start).ToList();

        if (ordered.Count > MaxWindows)
            throw new DomainException("RN-AGD-01", $"São permitidos no máximo {MaxWindows} períodos de atendimento.");

        foreach (var window in ordered)
        {
            if (window.Start >= window.End)
                throw new DomainException("RN-AGD-01", "Cada período precisa começar antes de terminar.");

            if (window.Length < session)
                throw new DomainException("RN-AGD-01", "Cada período precisa ter espaço para pelo menos uma sessão.");
        }

        for (var i = 1; i < ordered.Count; i++)
            if (ordered[i].Overlaps(ordered[i - 1]))
                throw new DomainException("RN-AGD-01", "Períodos do mesmo dia não podem se sobrepor.");

        TimeZoneId = timeZoneId;
        DefaultSessionMinutes = defaultSessionMinutes;
        BreakMinutes = breakMinutes;
        _windows.Clear();
        _windows.AddRange(ordered);
        UpdatedAt = now;
    }

    // RN-AGD-13: o horário cabe inteiro num período de atendimento, na hora local da profissional
    public bool Covers(DateTimeOffset startsAt, TimeSpan duration)
    {
        var localStart = TimeZoneInfo.ConvertTime(startsAt, TimeZones.Find(TimeZoneId));
        var localEnd = localStart + duration;

        if (localEnd.Date != localStart.Date)
            return false;

        var start = TimeOnly.FromDateTime(localStart.DateTime);
        var end = TimeOnly.FromDateTime(localEnd.DateTime);
        return _windows.Any(w => w.Weekday == localStart.DayOfWeek && w.Contains(start, end));
    }
}