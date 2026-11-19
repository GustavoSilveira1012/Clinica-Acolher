using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Auth;

public class AuthApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Sem_login_as_rotas_protegidas_respondem_401()
    {
        using var client = factory.CreateClient();
        using var resposta = await client.GetAsync("/api/v1/professionals");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Health_continua_publico()
    {
        using var client = factory.CreateClient();
        using var resposta = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_SEG_04_cookie_de_sessao_e_HttpOnly_Secure_e_SameSite_Strict()
    {
        var email = await UsuarioComumAsync();
        using var semPoteDeCookies = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
        });

        using var resposta = await semPoteDeCookies.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "senha-de-teste-123" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var cookie = resposta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("acolher.sessao="));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_permite_saber_quem_esta_logado()
    {
        var email = await UsuarioComumAsync();
        using var client = await factory.CreateLoggedInClientAsync(email, "senha-de-teste-123");

        var eu = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");

        Assert.Equal(email, eu.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Senha_errada_e_email_inexistente_recebem_a_mesma_resposta()
    {
        using var client = factory.CreateClient();

        using var senhaErrada = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = ApiFactory.AdminEmail, password = "senha-errada-123" });
        using var emailInexistente = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "ninguem@exemplo.org", password = "senha-errada-123" });

        Assert.Equal(HttpStatusCode.Unauthorized, senhaErrada.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);
        var erroSenha = await senhaErrada.Content.ReadFromJsonAsync<JsonObject>();
        var erroEmail = await emailInexistente.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(erroSenha);
        Assert.NotNull(erroEmail);

        // O identificador de correlação muda por requisição; todo o restante deve ser igual.
        erroSenha.Remove("traceId");
        erroEmail.Remove("traceId");
        Assert.True(JsonNode.DeepEquals(erroSenha, erroEmail),
            "Senha inválida e e-mail inexistente devem retornar o mesmo conteúdo de erro.");
    }

    [Fact]
    public async Task RN_SEG_02_bloqueia_depois_de_5_senhas_erradas()
    {
        const string email = "bloqueio@exemplo.org";
        const string senha = "senha-correta-123";
        await CriarUsuarioAsync(email, senha);
        using var client = factory.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            using var tentativa = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "senha-errada-123" });
            Assert.Equal(HttpStatusCode.Unauthorized, tentativa.StatusCode);
        }
        using var comASenhaCerta = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = senha });

        Assert.Equal(HttpStatusCode.Unauthorized, comASenhaCerta.StatusCode);
    }

    [Fact]
    public async Task Logout_encerra_a_sessao()
    {
        var email = await UsuarioComumAsync();
        using var client = await factory.CreateLoggedInClientAsync(email, "senha-de-teste-123");

        using var logout = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        using var depois = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, depois.StatusCode);
    }

    private async Task<string> UsuarioComumAsync()
    {
        var email = $"comum-{Guid.NewGuid():N}@exemplo.org";
        await factory.CreateUserAsync(email, "senha-de-teste-123");
        return email;
    }

    private async Task CriarUsuarioAsync(string email, string senha)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await users.CreateAsync(new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true }, senha);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
