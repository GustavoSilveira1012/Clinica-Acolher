namespace ClinicaAcolher.Infrastructure.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Professional = "Professional";
    public const string Patient = "Patient";

    public static readonly string[] All = [Admin, Professional, Patient];
}