using ClinicaAcolher.Domain.Availability;
using ClinicaAcolher.Domain.Common;
using static ClinicaAcolher.UnitTests.Cenarios;

namespace ClinicaAcolher.UnitTests.Availability;

public class AvailabilityTests
{
    private const string SaoPaulo = "America/Sao_Paulo";

    // Segunda com pausa para o almoço, e quarta só de manhã
    private static readonly AvailabilityWindow[] Semana =
    [
        new(DayOfWeek.Monday, new(9, 0), new(12, 0)),
        new(DayOfWeek.Monday, new(14, 0), new(18, 0)),
        new(DayOfWeek.Wednesday, new(9, 0), new(12, 0)),
    ];

    private static ProfessionalAvailability Disponibilidade(params AvailabilityWindow[] periodos) =>
        ProfessionalAvailability.Create(ProfissionalAprovado(), SaoPaulo, 50, 10,
            periodos.Length > 0 ? periodos : Semana, Agora);

    // Segunda, 16/11/2026, num horário de Brasília (UTC−3)
    private static DateTimeOffset SegundaAs(int hora, int minuto = 0) =>
        new DateTimeOffset(2026, 11, 16, hora, minuto, 0, TimeSpan.FromHours(-3));

    private static readonly TimeSpan Sessao = TimeSpan.FromMinutes(50);

    [Fact]
    public void RN_AGD_01_aceita_periodos_com_pausa_e_os_guarda_em_ordem()
    {
        var disponibilidade = Disponibilidade(Semana.Reverse().ToArray());

        Assert.Equal(Semana, disponibilidade.Windows);
    }

    [Fact]
    public void RN_AGD_01_periodos_do_mesmo_dia_nao_se_sobrepoem()
    {
        var erro = Assert.Throws<DomainException>(() => Disponibilidade(
            new(DayOfWeek.Monday, new(9, 0), new(12, 0)),
            new(DayOfWeek.Monday, new(11, 0), new(13, 0))));

        Assert.Equal("RN-AGD-01", erro.Rule);
    }

    [Fact]
    public void Periodos_encostados_nao_contam_como_sobreposicao()
    {
        var disponibilidade = Disponibilidade(
            new(DayOfWeek.Monday, new(9, 0), new(12, 0)),
            new(DayOfWeek.Monday, new(12, 0), new(14, 0)));

        Assert.Equal(2, disponibilidade.Windows.Count);
    }

    [Fact]
    public void Periodo_menor_que_uma_sessao_e_recusado()
    {
        var erro = Assert.Throws<DomainException>(() =>
            Disponibilidade(new AvailabilityWindow(DayOfWeek.Monday, new(9, 0), new(9, 30))));

        Assert.Equal("RN-AGD-01", erro.Rule);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(61)]
    public void RN_AGD_01_intervalo_entre_sessoes_fica_entre_0_e_60_minutos(int intervalo)
    {
        var erro = Assert.Throws<DomainException>(() =>
            ProfessionalAvailability.Create(ProfissionalAprovado(), SaoPaulo, 50, intervalo, Semana, Agora));

        Assert.Equal("RN-AGD-01", erro.Rule);
    }

    [Fact]
    public void RN_AGD_13_sessao_dentro_do_periodo_esta_coberta()
    {
        Assert.True(Disponibilidade().Covers(SegundaAs(10), Sessao));
    }

    [Theory]
    [InlineData(12, 30)] // na pausa do almoço
    [InlineData(11, 30)] // começa dentro, mas termina às 12h20, depois do fim do período
    [InlineData(8, 30)]  // começa antes do período
    public void RN_AGD_13_sessao_fora_dos_periodos_nao_esta_coberta(int hora, int minuto)
    {
        Assert.False(Disponibilidade().Covers(SegundaAs(hora, minuto), Sessao));
    }

    [Fact]
    public void Dia_sem_periodo_de_atendimento_nao_esta_coberto()
    {
        Assert.False(Disponibilidade().Covers(SegundaAs(10).AddDays(1), Sessao));
    }

    [Fact]
    public void RN_AGD_13_vale_o_dia_da_semana_na_hora_local_da_profissional()
    {
        var noite = Disponibilidade(new AvailabilityWindow(DayOfWeek.Monday, new(20, 0), new(22, 0)));

        // 00h30 de terça em UTC é 21h30 de segunda em Brasília
        var tercaEmUtc = new DateTimeOffset(2026, 11, 17, 0, 30, 0, TimeSpan.Zero);

        Assert.True(noite.Covers(tercaEmUtc, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void RN_AGD_02_bloqueio_precisa_terminar_depois_de_comecar()
    {
        var erro = Assert.Throws<DomainException>(() =>
            AvailabilityBlock.Create(ProfissionalAprovado(), SegundaAs(10), SegundaAs(9), "Férias", Agora));

        Assert.Equal("RN-AGD-02", erro.Rule);
    }

    [Fact]
    public void RN_AGD_02_bloqueio_detecta_sessoes_no_periodo()
    {
        var bloqueio = AvailabilityBlock.Create(ProfissionalAprovado(), SegundaAs(9), SegundaAs(13), "Congresso", Agora);

        Assert.True(bloqueio.Overlaps(SegundaAs(10), SegundaAs(10, 50)));
        Assert.False(bloqueio.Overlaps(SegundaAs(13), SegundaAs(13, 50)));
    }
}
