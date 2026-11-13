using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.IntegrationTests.Professionals;

public class ProfessionalsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Lista_os_profissionais_cadastrados()
    {
        var criado = await CriarAsync(EmailUnico());

        var lista = await _client.GetFromJsonAsync<List<ProfessionalResponse>>(
            "/api/v1/professionals", Json);

        Assert.Contains(lista!, p => p.Id == criado.Id);
    }

    [Fact]
    public async Task RN_PRO_04_profissional_fica_pendente_ate_ser_aprovado()
    {
        var criado = await CriarAsync(EmailUnico());
        Assert.Equal(VerificationStatus.Pending, criado.Verification);
        Assert.False(criado.CanAttend);

        var resposta = await _client.PostAsync($"/api/v1/professionals/{criado.Id}/approve", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var aprovado = await resposta.Content.ReadFromJsonAsync<ProfessionalResponse>(Json);
        Assert.True(aprovado!.CanAttend);
    }

    [Fact]
    public async Task RN_PRO_01_email_duplicado_retorna_422()
    {
        var email = EmailUnico();
        await CriarAsync(email);

        var resposta = await _client.PostAsJsonAsync("/api/v1/professionals", Pedido(email.ToUpperInvariant()), Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RN-PRO-01", problema.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RN_PRO_02_psicologo_sem_crp_retorna_422()
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/professionals",
            Pedido(EmailUnico(), ProfessionalCategory.Psychologist), Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RN-PRO-02", problema.GetProperty("code").GetString());
    }

    private static string EmailUnico() => $"prof-{Guid.NewGuid():N}@exemplo.org";

    private static CreateProfessionalRequest Pedido(
        string email, ProfessionalCategory categoria = ProfessionalCategory.Psychoanalyst) =>
        new("Profissional Teste", "Teste", email, categoria, null);

    private async Task<ProfessionalResponse> CriarAsync(string email)
    {
        var resposta = await _client.PostAsJsonAsync("/api/v1/professionals", Pedido(email), Json);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<ProfessionalResponse>(Json))!;
    }
}