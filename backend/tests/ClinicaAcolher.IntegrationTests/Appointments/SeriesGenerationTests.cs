using System.Net.Http.Json;
using ClinicaAcolher.Application.Appointments;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Appointments;

public class SeriesGenerationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Rotina_gera_a_proxima_semana_quando_o_tempo_passa()
    {
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
}