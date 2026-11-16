using System.Net.Http.Json;
using ClinicaAcolher.Application.Appointments;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Appointments;

public class SeriesGenerationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.AdminClient;

    [Fact]
    public async Task Rotina_gera_a_proxima_semana_quando_o_tempo_passa()
    {
        await factory.LoginAsync(_client, ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var vinculo = await _client.IniciarVinculoAsync(
            await _client.CriarPacienteAsync(), await _client.CriarProfissionalAprovadoAsync());
        var resposta = await _client.PostAsJsonAsync("/api/v1/series", new CreateSeriesRequest(
            vinculo, DayOfWeek.Tuesday, new TimeOnly(15, 0), 50, "America/Sao_Paulo", new DateOnly(2026, 11, 16)),
            TestApi.Json);
        var serie = (await resposta.Content.ReadFromJsonAsync<SeriesResponse>(TestApi.Json))!;

        factory.Clock.Advance(TimeSpan.FromDays(7));

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SeriesService>();
        Assert.Contains(serie.Id, await service.ListDueAsync(CancellationToken.None));
        Assert.Equal(1, await service.GenerateAsync(serie.Id, CancellationToken.None));
        Assert.Equal(0, await service.GenerateAsync(serie.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Serie_de_vinculo_suspenso_nao_aparece_como_pendente()
    {
        // Outros testes desta fixture também avançam o relógio e expiram o cookie.
        await factory.LoginAsync(_client, ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        var vinculo = await _client.IniciarVinculoAsync(
            await _client.CriarPacienteAsync(), await _client.CriarProfissionalAprovadoAsync());
        using var resposta = await _client.PostAsJsonAsync("/api/v1/series", new CreateSeriesRequest(
            vinculo, DayOfWeek.Wednesday, new TimeOnly(10, 0), 50, "America/Sao_Paulo", new DateOnly(2026, 11, 18)),
            TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        var serie = (await resposta.Content.ReadFromJsonAsync<SeriesResponse>(TestApi.Json))!;
        using var suspensao = await _client.PostAsync($"/api/v1/relationships/{vinculo}/suspend", null);
        suspensao.EnsureSuccessStatusCode();

        factory.Clock.Advance(TimeSpan.FromDays(7));
        using var scope = factory.Services.CreateScope();
        var pendentes = await scope.ServiceProvider.GetRequiredService<SeriesService>().ListDueAsync(CancellationToken.None);

        Assert.DoesNotContain(serie.Id, pendentes);
    }
}
