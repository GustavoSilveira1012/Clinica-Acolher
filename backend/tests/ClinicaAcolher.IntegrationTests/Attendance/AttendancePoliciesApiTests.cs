using System.Net.Http.Json;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Domain.Attendance;

namespace ClinicaAcolher.IntegrationTests.Attendance;

public class AttendancePoliciesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RN_CAN_01_cada_publicacao_cria_uma_nova_versao()
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        await PublicarAsync(profissional, falta: 100);
        var segunda = await PublicarAsync(profissional, falta: 50);

        var atual = await _client.GetFromJsonAsync<AttendancePolicyResponse>(
            $"/api/v1/professionals/{profissional}/attendance-policies/current", TestApi.Json);

        Assert.Equal(2, segunda.Version);
        Assert.Equal(segunda.Id, atual!.Id);
    }

    [Fact]
    public async Task Aceitar_a_mesma_versao_duas_vezes_nao_duplica()
    {
        var paciente = await _client.CriarPacienteAsync();
        var politica = await PublicarAsync(await _client.CriarProfissionalAprovadoAsync(), falta: 100);

        var primeiro = await AceitarAsync(politica.Id, paciente);
        var segundo = await AceitarAsync(politica.Id, paciente);

        Assert.Equal(primeiro.Id, segundo.Id);
    }

    [Fact]
    public async Task Agenda_mostra_cobranca_de_falta_quando_ha_aceite()
    {
        var (profissional, sessao) = await PrepararFaltaAsync(aceitar: true);

        var item = await ItemDaAgendaAsync(profissional, sessao);

        Assert.Equal(100, item.ChargePercent);
        Assert.Equal(ChargeReason.NoShow, item.ChargeReason);
    }

    [Fact]
    public async Task RN_CAN_07_agenda_nao_cobra_falta_sem_aceite()
    {
        var (profissional, sessao) = await PrepararFaltaAsync(aceitar: false);

        var item = await ItemDaAgendaAsync(profissional, sessao);

        Assert.Equal(0, item.ChargePercent);
        Assert.Equal(ChargeReason.None, item.ChargeReason);
    }

    private async Task<(Guid Profissional, AppointmentResponse Sessao)> PrepararFaltaAsync(bool aceitar)
    {
        var profissional = await _client.CriarProfissionalAprovadoAsync();
        var paciente = await _client.CriarPacienteAsync();
        var vinculo = await _client.IniciarVinculoAsync(paciente, profissional);
        var politica = await PublicarAsync(profissional, falta: 100);
        if (aceitar)
            await AceitarAsync(politica.Id, paciente);

        // A sessão é amanhã, pelo relógio falso
        var inicio = factory.Clock.GetUtcNow().AddDays(1);
        var agendamento = await _client.PostAsJsonAsync("/api/v1/appointments",
            new ScheduleAppointmentRequest(vinculo, inicio, 50), TestApi.Json);
        var sessao = (await agendamento.Content.ReadFromJsonAsync<AppointmentResponse>(TestApi.Json))!;

        // O tempo passa: chegamos a 20 minutos depois do início, e o paciente não apareceu
        factory.Clock.Advance(TimeSpan.FromDays(1) + TimeSpan.FromMinutes(20));
        (await _client.PostAsync($"/api/v1/appointments/{sessao.Id}/no-show", null)).EnsureSuccessStatusCode();

        return (profissional, sessao);
    }

    private async Task<AgendaItemResponse> ItemDaAgendaAsync(Guid profissional, AppointmentResponse sessao)
    {
        var de = Uri.EscapeDataString(sessao.StartsAt.AddHours(-1).ToString("O"));
        var ate = Uri.EscapeDataString(sessao.StartsAt.AddHours(2).ToString("O"));

        var agenda = await _client.GetFromJsonAsync<List<AgendaItemResponse>>(
            $"/api/v1/professionals/{profissional}/agenda?from={de}&to={ate}", TestApi.Json);

        return Assert.Single(agenda!);
    }

    private async Task<AttendancePolicyResponse> PublicarAsync(Guid profissional, int falta)
    {
        var resposta = await _client.PostAsJsonAsync($"/api/v1/professionals/{profissional}/attendance-policies",
            new PublishAttendancePolicyRequest(24, 100, falta), TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<AttendancePolicyResponse>(TestApi.Json))!;
    }

    private async Task<PolicyAcceptanceResponse> AceitarAsync(Guid politica, Guid paciente)
    {
        var resposta = await _client.PostAsJsonAsync($"/api/v1/attendance-policies/{politica}/acceptances",
            new AcceptPolicyRequest(paciente), TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PolicyAcceptanceResponse>(TestApi.Json))!;
    }
}