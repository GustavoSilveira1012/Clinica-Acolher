using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.UnitTests.Relationships;

public class TherapeuticRelationshipTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 12, 15, 0, 0, TimeSpan.Zero);

    private static Patient PacienteAdulto() =>
        Patient.Create("Paciente Teste", new DateOnly(1990, 5, 20), Agora);

    private static Professional ProfissionalAprovado()
    {
        var profissional = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);
        profissional.Approve(Agora);
        return profissional;
    }

    [Fact]
    public void RN_VIN_01_inicia_vinculo_ativo_com_os_ids_corretos()
    {
        var paciente = PacienteAdulto();
        var profissional = ProfissionalAprovado();

        var vinculo = TherapeuticRelationship.Start(paciente, profissional, Agora);

        Assert.Equal(RelationshipStatus.Active, vinculo.Status);
        Assert.Equal(paciente.Id, vinculo.PatientId);
        Assert.Equal(profissional.Id, vinculo.ProfessionalId);
        Assert.True(vinculo.AllowsScheduling);
    }

    [Fact]
    public void RN_VIN_01_nao_inicia_com_profissional_pendente()
    {
        var pendente = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);

        var erro = Assert.Throws<DomainException>(() =>
            TherapeuticRelationship.Start(PacienteAdulto(), pendente, Agora));

        Assert.Equal("RN-VIN-01", erro.Rule);
    }

    [Fact]
    public void RN_VIN_01_nao_inicia_com_paciente_desativado()
    {
        var paciente = PacienteAdulto();
        paciente.Deactivate("Encerrou o acompanhamento", Agora);

        var erro = Assert.Throws<DomainException>(() =>
            TherapeuticRelationship.Start(paciente, ProfissionalAprovado(), Agora));

        Assert.Equal("RN-VIN-01", erro.Rule);
    }

    [Fact]
    public void RN_PAC_03_menor_sem_responsavel_nao_inicia_vinculo()
    {
        var menor = Patient.Create("Paciente Teste", new DateOnly(2012, 3, 10), Agora);

        var erro = Assert.Throws<DomainException>(() =>
            TherapeuticRelationship.Start(menor, ProfissionalAprovado(), Agora));

        Assert.Equal("RN-PAC-03", erro.Rule);
    }

    [Fact]
    public void RN_VIN_03_suspende_e_retoma()
    {
        var vinculo = TherapeuticRelationship.Start(PacienteAdulto(), ProfissionalAprovado(), Agora);

        vinculo.Suspend();
        Assert.False(vinculo.AllowsScheduling);

        vinculo.Resume();
        Assert.Equal(RelationshipStatus.Active, vinculo.Status);
    }

    [Fact]
    public void RN_VIN_03_vinculo_encerrado_nao_volta()
    {
        var vinculo = TherapeuticRelationship.Start(PacienteAdulto(), ProfissionalAprovado(), Agora);
        vinculo.End("Encerrou o acompanhamento", Agora);

        Assert.Equal("RN-VIN-03", Assert.Throws<DomainException>(() => vinculo.Resume()).Rule);
        Assert.Equal("RN-VIN-03", Assert.Throws<DomainException>(() => vinculo.End("De novo", Agora)).Rule);
    }

    [Fact]
    public void RN_VIN_04_encerrar_exige_motivo()
    {
        var vinculo = TherapeuticRelationship.Start(PacienteAdulto(), ProfissionalAprovado(), Agora);

        var erro = Assert.Throws<DomainException>(() => vinculo.End("  ", Agora));

        Assert.Equal("RN-VIN-04", erro.Rule);
        Assert.Equal(RelationshipStatus.Active, vinculo.Status);
    }

    [Fact]
    public void RN_VIN_04_encerrar_registra_data_e_motivo()
    {
        var vinculo = TherapeuticRelationship.Start(PacienteAdulto(), ProfissionalAprovado(), Agora);

        vinculo.End("  Mudou de cidade  ", Agora.AddDays(90));

        Assert.Equal(RelationshipStatus.Ended, vinculo.Status);
        Assert.Equal(Agora.AddDays(90), vinculo.EndedAt);
        Assert.Equal("Mudou de cidade", vinculo.EndReason);
        Assert.False(vinculo.AllowsScheduling);
    }
}