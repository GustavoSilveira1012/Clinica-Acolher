namespace ClinicaAcolher.Domain.Common;

public static class TimeZones
{
    public static TimeZoneInfo Find(string timeZoneId)
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