namespace ClinicaAcolher.Application.Common;

public sealed class ConflictException(string rule, string message) : Exception(message)
{
    public string Rule {get;} = rule;
}