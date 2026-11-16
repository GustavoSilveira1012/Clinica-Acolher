using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Relationships;

namespace ClinicaAcolher.IntegrationTests.Auth;

public class PatientAccessTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateOnly Nascimento = new(1990, 5, 20);

    [Fact]
    public async Task RN_PER_01_profissional_nao_ve_paciente_de_outro_profissional()
    {
        var (_, profissionalA) = await factory.CriarProfissionalComLoginAsync();
        var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
        using (profissionalA)
        using (profissionalB)
        {
            var paciente = await CadastrarAsync(profissionalA, "Paciente da Profissional A");

            using var pelaA = await profissionalA.GetAsync($"/api/v1/patients/{paciente}");
            using var pelaB = await profissionalB.GetAsync($"/api/v1/patients/{paciente}");

            Assert.Equal(HttpStatusCode.OK, pelaA.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, pelaB.StatusCode);
        }
    }

    [Fact]
    public async Task RN_SEG_05_paciente_alheio_e_paciente_inexistente_recebem_a_mesma_resposta()
    {
        var (_, profissionalA) = await factory.CriarProfissionalComLoginAsync();
        var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
        using (profissionalA)
        using (profissionalB)
        {
            var paciente = await CadastrarAsync(profissionalA, "Paciente Sigiloso");

            using var alheio = await profissionalB.PostAsJsonAsync(
                $"/api/v1/patients/{paciente}/deactivate",
                new DeactivatePatientRequest("Tentativa indevida"), TestApi.Json);
            using var inexistente = await profissionalB.PostAsJsonAsync(
                $"/api/v1/patients/{Guid.NewGuid()}/deactivate",
                new DeactivatePatientRequest("Tentativa indevida"), TestApi.Json);

            Assert.Equal(HttpStatusCode.NotFound, alheio.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);

            var corpoAlheio = JsonNode.Parse(await alheio.Content.ReadAsStringAsync());
            var corpoInexistente = JsonNode.Parse(await inexistente.Content.ReadAsStringAsync());
            corpoAlheio?.AsObject().Remove("traceId");
            corpoInexistente?.AsObject().Remove("traceId");
            Assert.True(JsonNode.DeepEquals(corpoInexistente, corpoAlheio));
        }
    }

    [Fact]
    public async Task RN_PER_01_a_busca_so_traz_os_proprios_pacientes()
    {
        var (_, profissionalA) = await factory.CriarProfissionalComLoginAsync();
        var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
        using (profissionalA)
        using (profissionalB)
        {
            var marcador = Guid.NewGuid().ToString("N")[..8];
            await CadastrarAsync(profissionalA, $"Paciente {marcador} A");
            await CadastrarAsync(profissionalB, $"Paciente {marcador} B");

            using var busca = await profissionalA.PostAsJsonAsync(
                "/api/v1/patients/search", new PatientSearchRequest(marcador, null), TestApi.Json);
            busca.EnsureSuccessStatusCode();
            var resultado = await busca.Content.ReadFromJsonAsync<PagedResult<PatientListItem>>(TestApi.Json);

            var unico = Assert.Single(resultado!.Items);
            Assert.EndsWith(" A", unico.DisplayName);
        }
    }

    [Fact]
    public async Task RN_PRO_04_profissional_nao_aprova_profissional()
    {
        var (_, profissional) = await factory.CriarProfissionalComLoginAsync();
        using (profissional)
        {
            var outro = await factory.AdminClient.CriarProfissionalAprovadoAsync();

            using var resposta = await profissional.PostAsync($"/api/v1/professionals/{outro}/approve", null);

            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        }
    }

    private static async Task<Guid> CadastrarAsync(HttpClient profissional, string nome)
    {
        using var resposta = await profissional.PostAsJsonAsync(
            "/api/v1/patients",
            new CreatePatientRequest(nome, Nascimento, null, null, null),
            TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PatientResponse>(TestApi.Json))!.Id;
    }
}
