using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;
using static ClinicaAcolher.UnitTests.Cenarios;

namespace ClinicaAcolher.UnitTests.Appointments;

public class AppointmentSeriesTests
{
    private const string SaoPaulo = "America/Sao_Paulo";
    private static readonly DateOnly Segunda16 = new(2026, 11, 16);

    private static TherapeuticRelationship Vinculo() =>
        TherapeuticRelationship.Start(Paciente(), ProfissionalAprovado(), Agora);

    private static AppointmentSeries TercasAs15(TherapeuticRelationship vinculo, DateOnly inicio) =>
        AppointmentSeries.Create(vinculo, DayOfWeek.Tuesday, new TimeOnly(15, 0), 50, SaoPaulo, inicio, Agora);

    [Fact]
    public void RN_AGD_12_gera_toda_terca_as_15h_de_Brasilia()
    {
        var serie = TercasAs15(Vinculo(), Segunda16);

        var ocorrencias = serie.OccurrencesBetween(Segunda16, new DateOnly(2026, 12, 7)).ToList();

        Assert.Equal(3, ocorrencias.Count);
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 18, 0, 0, TimeSpan.Zero), ocorrencias[0]);
        Assert.All(ocorrencias, o => Assert.Equal(DayOfWeek.Tuesday, o.AddHours(-3).DayOfWeek));
    }

    [Fact]
    public void RN_AGD_12_mantem_a_hora_local_quando_o_horario_de_verao_termina()
    {
        var serie = AppointmentSeries.Create(Vinculo(), DayOfWeek.Tuesday, new TimeOnly(10, 0), 50,
            "America/New_York", new DateOnly(2026, 10, 26), Agora);

        var ocorrencias = serie.OccurrencesBetween(new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 8)).ToList();

        Assert.Equal(14, ocorrencias[0].Hour); // 27/10, horário de verão: 10h em Nova York = 14h UTC
        Assert.Equal(15, ocorrencias[1].Hour); // 03/11, horário normal:  10h em Nova York = 15h UTC
    }

    [Fact]
    public void Serie_comecada_numa_quarta_gera_a_primeira_sessao_na_terca_seguinte()
    {
        var quarta18 = new DateOnly(2026, 11, 18);
        var serie = TercasAs15(Vinculo(), quarta18);

        var primeira = serie.OccurrencesBetween(quarta18, new DateOnly(2026, 12, 7)).First();

        Assert.Equal(new DateTimeOffset(2026, 11, 24, 18, 0, 0, TimeSpan.Zero), primeira);
    }

    [Fact]
    public void RN_AGD_05_geracao_e_idempotente()
    {
        var vinculo = Vinculo();
        var serie = TercasAs15(vinculo, Segunda16);

        var primeiraLeva = serie.GenerateUntil(vinculo, new DateOnly(2026, 12, 7), Agora);
        var repetida = serie.GenerateUntil(vinculo, new DateOnly(2026, 12, 7), Agora);
        var proximaSemana = serie.GenerateUntil(vinculo, new DateOnly(2026, 12, 14), Agora);

        Assert.Equal(3, primeiraLeva.Count);
        Assert.All(primeiraLeva, s => Assert.Equal(serie.Id, s.SeriesId));
        Assert.Empty(repetida);
        Assert.Single(proximaSemana);
    }

    [Fact]
    public void RN_VIN_04_nao_cria_serie_com_vinculo_suspenso()
    {
        var vinculo = Vinculo();
        vinculo.Suspend();

        var erro = Assert.Throws<DomainException>(() => TercasAs15(vinculo, Segunda16));

        Assert.Equal("RN-VIN-04", erro.Rule);
    }

    [Fact]
    public void Vinculo_suspenso_depois_da_criacao_apenas_pausa_a_geracao()
    {
        var vinculo = Vinculo();
        var serie = TercasAs15(vinculo, Segunda16);
        vinculo.Suspend();

        Assert.Empty(serie.GenerateUntil(vinculo, new DateOnly(2026, 12, 7), Agora));
        Assert.Null(serie.GeneratedUntil);
    }

    [Fact]
    public void RN_AGD_09_rejeita_fuso_desconhecido()
    {
        var erro = Assert.Throws<DomainException>(() =>
            AppointmentSeries.Create(Vinculo(), DayOfWeek.Tuesday, new TimeOnly(15, 0), 50,
                "Planeta/Marte", Segunda16, Agora));

        Assert.Equal("RN-AGD-09", erro.Rule);
    }

    [Fact]
    public void Serie_encerrada_nao_gera_sessoes_depois_do_ultimo_dia()
    {
        var serie = TercasAs15(Vinculo(), Segunda16);

        serie.End(new DateOnly(2026, 11, 24));

        Assert.False(serie.IsActive);
        Assert.Equal(2, serie.OccurrencesBetween(Segunda16, new DateOnly(2026, 12, 7)).Count());
    }
}