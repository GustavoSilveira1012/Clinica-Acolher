using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.UnitTests;

public static class Cenarios
{
    public static readonly DateTimeOffset Agora = new(2026, 11, 13, 15, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Inicio = Agora.AddDays(7);

    public static Patient Paciente() => Patient.Create("Paciente Teste", new DateOnly(1990, 5, 20), Agora);

    public static Professional ProfissionalAprovado()
    {
        var profissional = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);
        profissional.Approve(Agora);
        return profissional;
    }

    public static Appointment Sessao(Patient paciente, Professional profissional) =>
        Appointment.Schedule(
            TherapeuticRelationship.Start(paciente, profissional, Agora),
            Inicio, TimeSpan.FromMinutes(50), Agora);
}