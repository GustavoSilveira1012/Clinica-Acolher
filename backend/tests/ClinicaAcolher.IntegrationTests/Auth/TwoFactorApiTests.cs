using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicaAcolher.Infrastructure.Identity;
using OtpNet;

namespace ClinicaAcolher.IntegrationTests.Auth;

public class TwoFactorApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Senha = "senha-correta-123";

    [Fact]
    public async Task RN_SEG_03_profissional_sem_2fa_so_acessa_a_configuracao()
    {
        var email = await CriarProfissionalAsync();
        var client = await factory.CreateLoggedInClientAsync(email, Senha);

        var dados = await client.GetAsync("/api/v1/professionals");
        var eu = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, dados.StatusCode);
        Assert.True(eu.GetProperty("mustSetupTwoFactor").GetBoolean());
    }

    [Fact]
    public async Task Ativar_2fa_libera_o_acesso_e_entrega_10_codigos_de_recuperacao()
    {
        var email = await CriarProfissionalAsync();
        var client = await factory.CreateLoggedInClientAsync(email, Senha);

        var (_, codigos) = await AtivarDoisFatoresAsync(client);

        Assert.Equal(10, codigos.Count);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/professionals")).StatusCode);
    }

    [Fact]
    public async Task Com_2fa_ativo_a_senha_sozinha_nao_basta()
    {
        var email = await CriarProfissionalAsync();
        var (chave, _) = await AtivarDoisFatoresAsync(await factory.CreateLoggedInClientAsync(email, Senha));
        var client = factory.CreateClient();

        var login = await (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Senha }))
            .Content.ReadFromJsonAsync<JsonElement>();
        var antesDoCodigo = await client.GetAsync("/api/v1/professionals");
        var comCodigo = await client.PostAsJsonAsync("/api/v1/auth/login/2fa", new { code = CodigoAtual(chave) });
        var depoisDoCodigo = await client.GetAsync("/api/v1/professionals");

        Assert.True(login.GetProperty("requiresTwoFactor").GetBoolean());
        Assert.Equal(HttpStatusCode.Unauthorized, antesDoCodigo.StatusCode);
        Assert.Equal(HttpStatusCode.OK, comCodigo.StatusCode);
        Assert.Equal(HttpStatusCode.OK, depoisDoCodigo.StatusCode);
    }

    [Fact]
    public async Task Codigo_errado_nao_entra()
    {
        var email = await CriarProfissionalAsync();
        await AtivarDoisFatoresAsync(await factory.CreateLoggedInClientAsync(email, Senha));
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Senha });

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login/2fa", new { code = "000000" });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Codigo_de_recuperacao_funciona_uma_unica_vez()
    {
        var email = await CriarProfissionalAsync();
        var (_, codigos) = await AtivarDoisFatoresAsync(await factory.CreateLoggedInClientAsync(email, Senha));

        var primeiraVez = await EntrarComRecuperacaoAsync(email, codigos[0]);
        var segundaVez = await EntrarComRecuperacaoAsync(email, codigos[0]);

        Assert.Equal(HttpStatusCode.OK, primeiraVez);
        Assert.Equal(HttpStatusCode.Unauthorized, segundaVez);
    }

    private async Task<HttpStatusCode> EntrarComRecuperacaoAsync(string email, string codigo)
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Senha });
        return (await client.PostAsJsonAsync("/api/v1/auth/login/recovery", new { code = codigo })).StatusCode;
    }

    private async Task<string> CriarProfissionalAsync()
    {
        var email = $"prof-{Guid.NewGuid():N}@exemplo.org";
        await factory.CreateUserAsync(email, Senha, Roles.Professional);
        return email;
    }

    private static async Task<(string Chave, List<string> Codigos)> AtivarDoisFatoresAsync(HttpClient client)
    {
        var setup = await (await client.PostAsync("/api/v1/auth/2fa/setup", null))
            .Content.ReadFromJsonAsync<JsonElement>();
        var chave = setup.GetProperty("manualKey").GetString()!.Replace(" ", "").ToUpperInvariant();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/2fa/enable", new { code = CodigoAtual(chave) });
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        var codigos = corpo.GetProperty("recoveryCodes").EnumerateArray().Select(c => c.GetString()!).ToList();
        return (chave, codigos);
    }

    // Faz o papel do aplicativo autenticador do celular
    private static string CodigoAtual(string chave) => new Totp(Base32Encoding.ToBytes(chave)).ComputeTotp();
}