namespace ClinicaAcolher.Domain.Common;

public sealed class DomainException(string rule, string message) : Exception(message)
{
    public string Rule { get; } = rule;
}