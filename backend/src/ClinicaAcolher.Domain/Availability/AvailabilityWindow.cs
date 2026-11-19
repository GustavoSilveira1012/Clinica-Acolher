namespace ClinicaAcolher.Domain.Availability;

/// <summary>Um período de atendimento num dia da semana, em hora local. Ex.: segunda, das 09:00 às 12:00.</summary>
public sealed record AvailabilityWindow(DayOfWeek Weekday, TimeOnly Start, TimeOnly End)
{
    public TimeSpan Length => End - Start;

    public bool Contains(TimeOnly start, TimeOnly end) => Start <= start && end <= End;

    public bool Overlaps(AvailabilityWindow other) =>
        Weekday == other.Weekday && Start < other.End && other.Start < End;
}