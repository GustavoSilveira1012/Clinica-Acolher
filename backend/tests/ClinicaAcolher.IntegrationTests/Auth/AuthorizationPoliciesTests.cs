using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using ClinicaAcolher.Api.Auth;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Auth;

public class AuthorizationPoliciesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "senha-de-teste-123";

    [Theory]
    [InlineData(Roles.Patient)]
    [InlineData(null)]
    public async Task Paciente_ou_conta_sem_papel_nao_acessa_rota_padrao_da_equipe(string? role)
    {
        using var client = await CreateClientAsync(role);

        using var response = await client.GetAsync($"/api/v1/series/{Guid.NewGuid()}");
        using var me = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Paciente_nao_usa_a_rota_de_aceite_da_equipe_nem_em_seu_proprio_nome()
    {
        var patientId = await factory.AdminClient.CriarPacienteAsync();
        var professionalId = await factory.AdminClient.CriarProfissionalAprovadoAsync();
        using var published = await factory.AdminClient.PostAsJsonAsync(
            $"/api/v1/professionals/{professionalId}/attendance-policies",
            new PublishAttendancePolicyRequest(24, 100, 100));
        published.EnsureSuccessStatusCode();
        var policy = (await published.Content.ReadFromJsonAsync<AttendancePolicyResponse>())!;
        using var client = await CreateClientAsync(Roles.Patient, patientId);

        using var own = await client.PostAsJsonAsync($"/api/v1/attendance-policies/{policy.Id}/acceptances",
            new AcceptPolicyRequest(patientId));

        // O paciente aceita pelo portal, que lê a identidade da sessão (RN-POR-04)
        Assert.Equal(HttpStatusCode.Forbidden, own.StatusCode);
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Professional)]
    [InlineData(null)]
    public async Task Aceite_rejeita_equipe_sem_MFA_e_usuario_sem_papel(string? role)
    {
        var patientId = await factory.AdminClient.CriarPacienteAsync();
        using var client = await CreateClientAsync(role, patientId);

        using var response = await client.PostAsJsonAsync(
            $"/api/v1/attendance-policies/{Guid.NewGuid()}/acceptances", new AcceptPolicyRequest(patientId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(Roles.Patient, true, true)]
    [InlineData(Roles.Patient, false, false)]
    [InlineData(Roles.Admin, true, false)]
    [InlineData(Roles.Professional, true, false)]
    [InlineData(null, true, false)]
    public async Task Politica_do_portal_exige_paciente_autenticado(string? role, bool authenticated, bool allowed)
    {
        using var scope = factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var identity = new ClaimsIdentity(authenticated ? "Test" : null);
        if (role is not null)
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(identity), null, Policies.Patient);

        Assert.Equal(allowed, result.Succeeded);
    }

    private async Task<HttpClient> CreateClientAsync(string? role, Guid? patientId = null)
    {
        var email = $"politica-{Guid.NewGuid():N}@exemplo.org";
        await factory.CreateUserAsync(email, Password, role);
        if (patientId is not null)
        {
            using var scope = factory.Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByEmailAsync(email))!;
            user.PatientId = patientId;
            var update = await users.UpdateAsync(user);
            Assert.True(update.Succeeded, string.Join("; ", update.Errors.Select(e => e.Description)));
        }
        return await factory.CreateLoggedInClientAsync(email, Password);
    }
}
