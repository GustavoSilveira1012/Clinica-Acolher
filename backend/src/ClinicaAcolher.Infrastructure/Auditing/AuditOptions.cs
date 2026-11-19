namespace ClinicaAcolher.Infrastructure.Auditing;

public sealed class AuditOptions
{
    public const string Section = "Audit";

    public string HmacKey { get; set; } = string.Empty;

    public bool IsValid()
    {
        try
        {
            return Convert.FromBase64String(HmacKey).Length >= 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}