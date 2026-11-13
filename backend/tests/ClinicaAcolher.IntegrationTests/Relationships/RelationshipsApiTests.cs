using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.IntegrationTests.Relationships;

public class RelationshipsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RN_VIN_01_inicia_vinculo_ativo()
    {
        var resposta = await IniciarAsync(await CriarPacienteAsync(), await CriarProfissionalAsync(aprovar: true));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var vinculo = await resposta.Content.ReadFromJsonAsync<RelationshipResponse>(Json);
        Assert.Equal(RelationshipStatus.Active, vinculo!.Status);
    }

    [Fact]
    public async Task RN_VIN_01_profissional_pendente_retorna_422()
    {
        var resposta = await IniciarAsync(await CriarPacienteAsync(), await CriarProfissionalAsync(aprovar: false));

        await AssertProblemaAsync(resposta, HttpStatusCode.UnprocessableEntity, "RN-VIN-01");
    }

    [Fact]
    public async Task RN_VIN_02_segundo_vinculo_aberto_retorna_422()
    {
        var paciente = await CriarPacienteAsync();
        var profissional = await CriarProfissionalAsync(aprovar: true);
        await IniciarAsync(paciente, profissional);

        var resposta = await IniciarAsync(paciente, profissional);

        await AssertProblemaAsync(resposta, HttpStatusCode.UnprocessableEntity, "RN-VIN-02");
    }

    [Fact]
    public async Task RN_VIN_02_depois_de_encerrar_pode_iniciar_novo_vinculo()
    {
        var paciente = await CriarPacienteAsync();
        var profissional = await CriarProfissionalAsync(aprovar: true);
        var primeiro = await (await IniciarAsync(paciente, profissional))
            .Content.ReadFromJsonAsync<RelationshipResponse>(Json);

        await _client.PostAsJsonAsync($"/api/v1/relationships/{primeiro!.Id}/end",
            new EndRelationshipRequest("Encerrou o acompanhamento"), Json);
        var novo = await IniciarAsync(paciente, profissional);

        Assert.Equal(HttpStatusCode.Created, novo.StatusCode);
    }

    [Fact]
    public async Task RN_VIN_02_requisicoes_simultaneas_criam_um_unico_vinculo()
    {
        var paciente = await CriarPacienteAsync();
        var profissional = await CriarProfissionalAsync(aprovar: true);

        var respostas = await Task.WhenAll(
            IniciarAsync(paciente, profissional),
            IniciarAsync(paciente, profissional));

        Assert.Single(respostas, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(respostas, r => r.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Paciente_inexistente_retorna_404()
    {
        var resposta = await IniciarAsync(Guid.NewGuid(), await CriarProfissionalAsync(aprovar: true));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private Task<HttpResponseMessage> IniciarAsync(Guid pacienteId, Guid profissionalId) =>
        _client.PostAsJsonAsync("/api/v1/relationships", new StartRelationshipRequest(pacienteId, profissionalId), Json);

    private async Task<Guid> CriarPacienteAsync()
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Paciente Teste", new DateOnly(1990, 5, 20), null, null, null), Json);
        return (await resposta.Content.ReadFromJsonAsync<PatientResponse>(Json))!.Id;
    }

    private async Task<Guid> CriarProfissionalAsync(bool aprovar)
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/professionals",
            new CreateProfessionalRequest("Profissional Teste", "Teste", $"prof-{Guid.NewGuid():N}@exemplo.org",
                ProfessionalCategory.Psychoanalyst, null), Json);
        var id = (await resposta.Content.ReadFromJsonAsync<ProfessionalResponse>(Json))!.Id;

        if (aprovar)
            await _client.PostAsync($"/api/v1/professionals/{id}/approve", null);

        return id;
    }

    private static async Task AssertProblemaAsync(HttpResponseMessage resposta, HttpStatusCode status, string regra)
    {
        Assert.Equal(status, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(regra, problema.GetProperty("code").GetString());
    }
}