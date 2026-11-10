using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.UnitTests.Patients;

public class PatientTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 9, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Nascimento = new(1990, 5, 20);

    [Fact]
    public void RN_PAC_01_cria_paciente_com_dados_normalizados()
    {
        var paciente = Patient.Create("  Maria Teste  ", Nascimento, Agora, email: " MARIA@Exemplo.ORG ");

        Assert.Equal("Maria Teste", paciente.FullName);
        Assert.Equal("maria@exemplo.org", paciente.Email);
        Assert.True(paciente.Active);
        Assert.Equal(Agora, paciente.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void RN_PAC_01_rejeita_nome_invalido(string nome)
    {
        var erro = Assert.Throws<DomainException>(() => Patient.Create(nome, Nascimento, Agora));

        Assert.Equal("RN-PAC-01", erro.Rule);
    }

    [Fact]
    public void RN_PAC_01_rejeita_nascimento_no_futuro()
    {
        var erro = Assert.Throws<DomainException>(() =>
            Patient.Create("Maria Teste", new DateOnly(2030, 1, 1), Agora));

        Assert.Equal("RN-PAC-01", erro.Rule);
    }

    [Fact]
    public void RN_PAC_02_desativar_e_reativar_preserva_o_historico()
    {
        var paciente = Patient.Create("Maria Teste", Nascimento, Agora);

        paciente.Deactivate("Encerrou o tratamento", Agora.AddDays(30));
        paciente.Reactivate(Agora.AddDays(60));

        Assert.True(paciente.Active);
        Assert.Equal(Agora.AddDays(30), paciente.DeactivatedAt);
        Assert.Equal("Encerrou o tratamento", paciente.DeactivationReason);
        Assert.Equal(Agora.AddDays(60), paciente.ReactivatedAt);
    }

    [Fact]
    public void RN_PAC_02_nao_desativa_sem_motivo()
    {
        var paciente = Patient.Create("Maria Teste", Nascimento, Agora);

        var erro = Assert.Throws<DomainException>(() => paciente.Deactivate("  ", Agora));

        Assert.Equal("RN-PAC-02", erro.Rule);
        Assert.True(paciente.Active);
    }

    [Fact]
    public void RN_PAC_07_nome_social_substitui_o_nome_civil_na_exibicao()
    {
        var paciente = Patient.Create("Nome Civil Teste", Nascimento, Agora, socialName: "Nome Social");

        Assert.Equal("Nome Social", paciente.DisplayName);
    }

    [Theory]
    [InlineData(2008, 11, 10, true)]
    [InlineData(2008, 11, 9, false)]
    [InlineData(1990, 1, 1, false)]
    public void RN_PAC_03_identifica_menor_de_idade(int ano, int mes, int dia, bool esperado)
    {
        var paciente = Patient.Create("Maria Teste", new DateOnly(ano, mes, dia), Agora);

        Assert.Equal(esperado, paciente.IsMinor(new DateOnly(2026, 11, 9)));
    }
}