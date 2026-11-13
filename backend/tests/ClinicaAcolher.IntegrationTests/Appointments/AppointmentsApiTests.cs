using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Application.Appointments;

namespace ClinicaAcolher.IntegrationTests.Appointments;

public class AppointmentsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset Segunda15h = new(2030, 1, 7, 15, 0, 0, TimeSpan.Zero);
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Agenda_mostra_a_sessao_com_o_nome_do_paciente()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculo = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync("Paciente Agenda"), profissional);
        await AgendarAsync(vinculo, Segunda15h);

        var agenda = await _client.GetFromJsonAsync<List<AgendaItemResponse>>(
            $"/api/v1/professionals/{profissional}/agenda?from=2030-01-07T00:00:00Z&to=2030-01-08T00:00:00Z", TestApi.Json);

        var item = Assert.Single(agenda!);
        Assert.Equal("Paciente Agenda", item.PatientDisplayName);
    }

    [Fact]
    public async Task RN_AGD_03_profissional_nao_tem_duas_sessoes_no_mesmo_horario()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculoA = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var vinculoB = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        await AgendarAsync(vinculoA, Segunda15h);

        var resposta = await AgendarAsync(vinculoB, Segunda15h.AddMinutes(30));

        await AssertConflitoAsync(resposta, "RN-AGD-03");
    }

    [Fact]
    public async Task RN_AGD_04_paciente_nao_tem_duas_sessoes_no_mesmo_horario()
    {
        var paciente = await _client.CriarPacienteAsync();
        var vinculoA = await _client.IniciarVinculoAsync(paciente, await _client.CriarProfissionalAprovadoAsync());
        var vinculoB = await _client.IniciarVinculoAsync(paciente, await _client.CriarProfissionalAprovadoAsync());
        await AgendarAsync(vinculoA, Segunda15h);

        var resposta = await AgendarAsync(vinculoB, Segunda15h.AddMinutes(10));

        await AssertConflitoAsync(resposta, "RN-AGD-04");
    }

    [Fact]
    public async Task Sessoes_encostadas_nao_conflitam()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculoA = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var vinculoB = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);

        var primeira = await AgendarAsync(vinculoA, Segunda15h);
        var segunda = await AgendarAsync(vinculoB, Segunda15h.AddMinutes(50));

        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
    }

    [Fact]
    public async Task Periodo_invertido_na_agenda_retorna_400()
    {
        var resposta = await _client.GetAsync(
            $"/api/v1/professionals/{Guid.NewGuid()}/agenda?from=2030-01-08T00:00:00Z&to=2030-01-07T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> AgendarAsync(Guid vinculo, DateTimeOffset inicio) =>
        _client.PostAsJsonAsync("/api/v1/appointments", new ScheduleAppointmentRequest(vinculo, inicio, 50), TestApi.Json);

    private static async Task AssertConflitoAsync(HttpResponseMessage resposta, string regra)
    {
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(regra, problema.GetProperty("code").GetString());
    }
}