using System.Net;
using System.Net.Http.Json;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Availability;

namespace ClinicaAcolher.IntegrationTests.Availability;

public class AvailabilityApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Segunda com pausa para o almoço
    private static readonly AvailabilityWindowDto[] SegundaComPausa =
    [
        new(DayOfWeek.Monday, new(9, 0), new(12, 0)),
        new(DayOfWeek.Monday, new(14, 0), new(18, 0)),
    ];

    [Fact]
    public async Task RN_AGD_01_salva_e_le_os_periodos_com_pausa()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();

        (await SalvarAsync(client, id, SegundaComPausa)).EnsureSuccessStatusCode();
        var lida = await client.GetFromJsonAsync<AvailabilityResponse>(Disponibilidade(id), TestApi.Json);

        Assert.Equal(SegundaComPausa, lida!.Windows);
        Assert.Equal(50, lida.DefaultSessionMinutes);
    }

    [Fact]
    public async Task Salvar_de_novo_substitui_os_periodos_anteriores()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();
        await SalvarAsync(client, id, SegundaComPausa);

        AvailabilityWindowDto[] soQuarta = [new(DayOfWeek.Wednesday, new(9, 0), new(12, 0))];
        await SalvarAsync(client, id, soQuarta);
        var lida = await client.GetFromJsonAsync<AvailabilityResponse>(Disponibilidade(id), TestApi.Json);

        Assert.Equal(soQuarta, lida!.Windows);
    }

    [Fact]
    public async Task RN_AGD_01_periodos_sobrepostos_sao_recusados()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();

        var resposta = await SalvarAsync(client, id,
        [
            new(DayOfWeek.Monday, new(9, 0), new(12, 0)),
            new(DayOfWeek.Monday, new(11, 0), new(13, 0)),
        ]);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_AGD_13_a_verificacao_avisa_quando_o_horario_esta_fora_do_atendimento()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();
        await SalvarAsync(client, id, SegundaComPausa);

        var dentro = await VerificarAsync(client, id, "2026-11-16T13:00:00Z"); // 10h em Brasília
        var naPausa = await VerificarAsync(client, id, "2026-11-16T15:30:00Z"); // 12h30 em Brasília

        Assert.True(dentro.Configured);
        Assert.True(dentro.WithinAvailability);
        Assert.False(naPausa.WithinAvailability);
    }

    [Fact]
    public async Task Sem_disponibilidade_configurada_nao_ha_o_que_avisar()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();

        var verificacao = await VerificarAsync(client, id, "2026-11-16T13:00:00Z");

        Assert.False(verificacao.Configured);
    }

    [Fact]
    public async Task RN_AGD_14_bloqueio_informa_as_sessoes_que_caem_no_periodo()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        await c.Client.PostAsJsonAsync("/api/v1/appointments", new ScheduleAppointmentRequest(
            c.VinculoId, new DateTimeOffset(2026, 11, 16, 13, 0, 0, TimeSpan.Zero), 50), TestApi.Json);

        var resposta = await c.Client.PostAsJsonAsync($"/api/v1/professionals/{c.ProfissionalId}/availability-blocks",
            new CreateBlockRequest(
                new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 11, 16, 15, 0, 0, TimeSpan.Zero),
                "Congresso"), TestApi.Json);
        var criado = await resposta.Content.ReadFromJsonAsync<BlockCreatedResponse>(TestApi.Json);
        var verificacao = await VerificarAsync(c.Client, c.ProfissionalId, "2026-11-16T13:00:00Z");

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(1, criado!.AffectedAppointments);
        Assert.True(verificacao.Blocked);
    }

    [Fact]
    public async Task Bloqueio_removido_deixa_de_valer()
    {
        var (id, client) = await factory.CriarProfissionalComLoginAsync();
        var resposta = await client.PostAsJsonAsync($"/api/v1/professionals/{id}/availability-blocks",
            new CreateBlockRequest(
                new DateTimeOffset(2026, 12, 21, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2027, 1, 4, 0, 0, 0, TimeSpan.Zero),
                "Férias"), TestApi.Json);
        var criado = (await resposta.Content.ReadFromJsonAsync<BlockCreatedResponse>(TestApi.Json))!;

        var remocao = await client.DeleteAsync($"/api/v1/availability-blocks/{criado.Block.Id}");
        var verificacao = await VerificarAsync(client, id, "2026-12-28T13:00:00Z");

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);
        Assert.False(verificacao.Blocked);
    }

    [Fact]
    public async Task RN_PER_01_profissional_nao_ve_nem_altera_a_disponibilidade_de_outro()
    {
        var (idA, clientA) = await factory.CriarProfissionalComLoginAsync();
        var (_, clientB) = await factory.CriarProfissionalComLoginAsync();
        await SalvarAsync(clientA, idA, SegundaComPausa);

        var leitura = await clientB.GetAsync(Disponibilidade(idA));
        var alteracao = await SalvarAsync(clientB, idA, []);

        Assert.Equal(HttpStatusCode.NotFound, leitura.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, alteracao.StatusCode);
    }

    private static string Disponibilidade(Guid profissional) => $"/api/v1/professionals/{profissional}/availability";

    private static Task<HttpResponseMessage> SalvarAsync(HttpClient client, Guid profissional, AvailabilityWindowDto[] periodos) =>
        client.PutAsJsonAsync(Disponibilidade(profissional),
            new UpdateAvailabilityRequest("America/Sao_Paulo", 50, 10, periodos), TestApi.Json);

    private static async Task<AvailabilityCheckResponse> VerificarAsync(HttpClient client, Guid profissional, string inicio) =>
        (await client.GetFromJsonAsync<AvailabilityCheckResponse>(
            $"{Disponibilidade(profissional)}/check?startsAt={inicio}&durationMinutes=50", TestApi.Json))!;
}
