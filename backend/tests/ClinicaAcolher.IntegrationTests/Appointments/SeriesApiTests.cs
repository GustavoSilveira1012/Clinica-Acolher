using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.IntegrationTests.Appointments;

public class SeriesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateOnly Segunda16 = new(2026, 11, 16);
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RN_AGD_05_criar_serie_gera_as_proximas_12_semanas()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculo = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);

        var serie = await LerAsync(await CriarSerieAsync(vinculo));
        var semana = await AgendaAsync(profissional, "2026-11-16T00:00:00Z", "2026-11-23T00:00:00Z");

        Assert.Equal(new DateOnly(2027, 2, 5), serie.GeneratedUntil);
        var sessao = Assert.Single(semana);
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 18, 0, 0, TimeSpan.Zero), sessao.StartsAt);
    }

    [Fact]
    public async Task Serie_que_conflita_com_sessao_existente_nao_e_criada()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var outroVinculo = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var vinculo = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);

        // Sessão avulsa de outro paciente na terça, 1/12, às 15h de Brasília
        await _client.PostAsJsonAsync("/api/v1/appointments", new ScheduleAppointmentRequest(
            outroVinculo, new DateTimeOffset(2026, 12, 1, 18, 0, 0, TimeSpan.Zero), 50), TestApi.Json);

        var resposta = await CriarSerieAsync(vinculo);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RN-AGD-03", problema.GetProperty("code").GetString());
        Assert.Empty(await AgendaAsync(profissional, "2026-11-16T00:00:00Z", "2026-11-23T00:00:00Z"));
    }

    [Fact]
    public async Task Encerrar_serie_cancela_as_sessoes_depois_do_ultimo_dia()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculo = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var serie = await LerAsync(await CriarSerieAsync(vinculo));

        await _client.PostAsJsonAsync($"/api/v1/series/{serie.Id}/end",
            new EndSeriesRequest(new DateOnly(2026, 12, 1)), TestApi.Json);
        var agenda = await AgendaAsync(profissional, "2026-11-30T00:00:00Z", "2026-12-14T00:00:00Z");

        Assert.Equal(AppointmentStatus.Scheduled, agenda.Single(a => a.StartsAt.Day == 1).Status);
        Assert.Equal(AppointmentStatus.Cancelled, agenda.Single(a => a.StartsAt.Day == 8).Status);
        Assert.All(agenda, a => Assert.Equal(0, a.ChargePercent));
    }

    private Task<HttpResponseMessage> CriarSerieAsync(Guid vinculo) =>
        _client.PostAsJsonAsync("/api/v1/series", new CreateSeriesRequest(
            vinculo, DayOfWeek.Tuesday, new TimeOnly(15, 0), 50, "America/Sao_Paulo", Segunda16), TestApi.Json);

    private async Task<List<AgendaItemResponse>> AgendaAsync(Guid profissional, string de, string ate) =>
        (await _client.GetFromJsonAsync<List<AgendaItemResponse>>(
            $"/api/v1/professionals/{profissional}/agenda?from={de}&to={ate}", TestApi.Json))!;

    private static async Task<SeriesResponse> LerAsync(HttpResponseMessage resposta)
    {
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<SeriesResponse>(TestApi.Json))!;
    }
}