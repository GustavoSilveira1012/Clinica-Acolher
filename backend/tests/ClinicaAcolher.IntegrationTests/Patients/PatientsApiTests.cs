using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Application.Patients;

namespace ClinicaAcolher.IntegrationTests.Patients;

public class PatientsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateOnly Nascimento = new(1990, 5, 20);
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Cria_e_consulta_paciente()
    {
        var criado = await CriarAsync("Paciente Integração", "INTEG@Exemplo.org");

        var consulta = await _client.GetFromJsonAsync<PatientResponse>($"/api/v1/patients/{criado.Id}");

        Assert.Equal("Paciente Integração", consulta!.FullName);
        Assert.Equal("integ@exemplo.org", consulta.Email);
    }

    [Fact]
    public async Task Edicao_e_gravada_no_banco()
    {
        var criado = await CriarAsync("Nome Original");

        var edicao = await _client.PutAsJsonAsync($"/api/v1/patients/{criado.Id}",
            new UpdatePatientRequest("Nome Editado", Nascimento, null, null, null));
        Assert.Equal(HttpStatusCode.OK, edicao.StatusCode);

        var consulta = await _client.GetFromJsonAsync<PatientResponse>($"/api/v1/patients/{criado.Id}");
        Assert.Equal("Nome Editado", consulta!.FullName);
    }

    [Fact]
    public async Task RN_PAC_01_nome_invalido_retorna_422_com_codigo_da_regra()
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("A", Nascimento, null, null, null));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RN-PAC-01", problema.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Paciente_inexistente_retorna_404()
    {
        var resposta = await _client.GetAsync($"/api/v1/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private async Task<PatientResponse> CriarAsync(string nome, string? email = null)
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest(nome, Nascimento, null, email, null));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<PatientResponse>())!;
    }
}