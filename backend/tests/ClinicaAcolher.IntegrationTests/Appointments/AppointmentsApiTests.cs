using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Domain.Appointments;

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

    [Fact]
    public async Task RN_CAN_03_remarcar_para_horario_sobreposto_ao_original_funciona()
    {
        var vinculo = await _client.IniciarVinculoAsync(
            await _client.CriarPacienteAsync(), await _client.CriarProfissionalAprovadoAsync());
        var original = await LerAsync(await AgendarAsync(vinculo, Segunda15h));

        var resposta = await _client.PostAsJsonAsync($"/api/v1/appointments/{original.Id}/reschedule",
            new RescheduleAppointmentRequest(Segunda15h.AddMinutes(30), null), TestApi.Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var nova = await LerAsync(resposta);
        var antiga = await BuscarAsync(original.Id);
        Assert.Equal(AppointmentStatus.Rescheduled, antiga.Status);
        Assert.Equal(nova.Id, antiga.RescheduledToId);
        Assert.Equal(TimeSpan.FromMinutes(50), nova.EndsAt - nova.StartsAt);
    }

    [Fact]
    public async Task RN_CAN_03_remarcacao_para_horario_ocupado_desfaz_tudo()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculoA = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var vinculoB = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var sessaoA = await LerAsync(await AgendarAsync(vinculoA, Segunda15h));
        await AgendarAsync(vinculoB, Segunda15h.AddHours(2));

        var resposta = await _client.PostAsJsonAsync($"/api/v1/appointments/{sessaoA.Id}/reschedule",
            new RescheduleAppointmentRequest(Segunda15h.AddHours(2).AddMinutes(10), null), TestApi.Json);

        await AssertConflitoAsync(resposta, "RN-AGD-03");
        Assert.Equal(AppointmentStatus.Scheduled, (await BuscarAsync(sessaoA.Id)).Status);
    }

    [Fact]
    public async Task RN_CAN_06_cancelar_libera_o_horario()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var vinculoA = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var vinculoB = await _client.IniciarVinculoAsync(await _client.CriarPacienteAsync(), profissional);
        var sessaoA = await LerAsync(await AgendarAsync(vinculoA, Segunda15h));

        await _client.PostAsJsonAsync($"/api/v1/appointments/{sessaoA.Id}/cancel",
            new CancelAppointmentRequest(CancellationSource.Patient, "Imprevisto"), TestApi.Json);
        var nova = await AgendarAsync(vinculoB, Segunda15h);

        Assert.Equal(HttpStatusCode.Created, nova.StatusCode);
    }

    [Fact]
    public async Task RN_AGD_11_falta_antes_do_horario_retorna_422()
    {
        var vinculo = await _client.IniciarVinculoAsync(
            await _client.CriarPacienteAsync(), await _client.CriarProfissionalAprovadoAsync());
        var sessao = await LerAsync(await AgendarAsync(vinculo, Segunda15h));

        var resposta = await _client.PostAsync($"/api/v1/appointments/{sessao.Id}/no-show", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Sessao_passada_pode_ser_registrada_como_realizada()
    {
        var vinculo = await _client.IniciarVinculoAsync(
            await _client.CriarPacienteAsync(), await _client.CriarProfissionalAprovadoAsync());
        var sessao = await LerAsync(await AgendarAsync(vinculo, new DateTimeOffset(2024, 3, 4, 15, 0, 0, TimeSpan.Zero)));

        var resposta = await _client.PostAsync($"/api/v1/appointments/{sessao.Id}/complete", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(AppointmentStatus.Completed, (await LerAsync(resposta)).Status);
    }

    private static async Task<AppointmentResponse> LerAsync(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<AppointmentResponse>(TestApi.Json))!;

    private async Task<AppointmentResponse> BuscarAsync(Guid id) =>
        (await _client.GetFromJsonAsync<AppointmentResponse>($"/api/v1/appointments/{id}", TestApi.Json))!;

    private Task<HttpResponseMessage> AgendarAsync(Guid vinculo, DateTimeOffset inicio) =>
        _client.PostAsJsonAsync("/api/v1/appointments", new ScheduleAppointmentRequest(vinculo, inicio, 50), TestApi.Json);

    private static async Task AssertConflitoAsync(HttpResponseMessage resposta, string regra)
    {
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(regra, problema.GetProperty("code").GetString());
    }
}
