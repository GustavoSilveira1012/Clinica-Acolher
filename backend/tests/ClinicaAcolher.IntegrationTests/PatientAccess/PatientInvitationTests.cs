using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClinicaAcolher.Application.PatientAccess;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.PatientAccess;

public class PatientInvitationTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private const string Senha = "senha-do-paciente-123";

    // O teste de expiração avança o relógio compartilhado e também expira o cookie administrativo.
    public Task InitializeAsync() =>
        factory.LoginAsync(factory.AdminClient, ApiFactory.AdminEmail, ApiFactory.AdminPassword);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Profissional_convida_e_o_paciente_entra_no_portal()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var convite = await ConvidarAsync(profissional, paciente);

        var (resposta, portal) = await AceitarAsync(convite.Token);
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var eu = await portal.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");
        Assert.Equal("Patient", Assert.Single(eu.GetProperty("roles").EnumerateArray()).GetString());
        Assert.Equal(paciente, eu.GetProperty("patientId").GetGuid());
    }

    [Fact]
    public async Task RN_POR_01_o_convite_vale_uma_unica_vez()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var convite = await ConvidarAsync(profissional, paciente);

        var (primeira, _) = await AceitarAsync(convite.Token);
        Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);
        var (segunda, _) = await AceitarAsync(convite.Token);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, segunda.StatusCode);
    }

    [Fact]
    public async Task RN_POR_01_um_convite_novo_invalida_o_anterior()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var primeiro = await ConvidarAsync(profissional, paciente);
        var segundo = await ConvidarAsync(profissional, paciente);

        var (comOPrimeiro, _) = await AceitarAsync(primeiro.Token);
        var (comOSegundo, _) = await AceitarAsync(segundo.Token);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, comOPrimeiro.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, comOSegundo.StatusCode);
    }

    [Fact]
    public async Task RN_POR_01_o_convite_expira_em_72_horas()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var convite = await ConvidarAsync(profissional, paciente);

        factory.Clock.Advance(TimeSpan.FromHours(73));
        var (resposta, _) = await AceitarAsync(convite.Token);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_POR_02_paciente_sem_email_nao_recebe_convite()
    {
        var (_, profissional) = await factory.CriarProfissionalComLoginAsync();
        var cadastro = await profissional.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Paciente Sem Email", new DateOnly(1990, 5, 20), null, null, null), TestApi.Json);
        cadastro.EnsureSuccessStatusCode();
        var paciente = (await cadastro.Content.ReadFromJsonAsync<PatientResponse>(TestApi.Json))!.Id;

        var resposta = await profissional.PostAsync($"/api/v1/patients/{paciente}/invitations", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_PER_01_profissional_nao_convida_paciente_de_outro()
    {
        var (_, paciente, _) = await ProfissionalComPacienteAsync();
        var (_, outroProfissional) = await factory.CriarProfissionalComLoginAsync();

        var resposta = await outroProfissional.PostAsync($"/api/v1/patients/{paciente}/invitations", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_POR_03_o_banco_guarda_so_o_hash_do_codigo()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var convite = await ConvidarAsync(profissional, paciente);

        using var scope = factory.Services.CreateScope();
        var gravado = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .PatientInvitations.Where(i => i.PatientId == paciente).Select(i => i.TokenHash).SingleAsync();

        Assert.Equal(SHA256.HashData(Encoding.UTF8.GetBytes(convite.Token)), gravado);
        Assert.DoesNotContain(convite.Token, Encoding.UTF8.GetString(gravado));
    }

    [Fact]
    public async Task Paciente_que_ja_tem_acesso_nao_recebe_convite_novo()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var (aceite, _) = await AceitarAsync((await ConvidarAsync(profissional, paciente)).Token);
        Assert.Equal(HttpStatusCode.NoContent, aceite.StatusCode);

        var resposta = await profissional.PostAsync($"/api/v1/patients/{paciente}/invitations", null);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_POR_04_paciente_logado_nao_entra_na_area_da_clinica()
    {
        var (profissional, paciente, _) = await ProfissionalComPacienteAsync();
        var (aceite, portal) = await AceitarAsync((await ConvidarAsync(profissional, paciente)).Token);
        Assert.Equal(HttpStatusCode.NoContent, aceite.StatusCode);

        var profissionais = await portal.GetAsync("/api/v1/professionals");
        var pacientes = await portal.PostAsJsonAsync("/api/v1/patients/search",
            new PatientSearchRequest(null, null), TestApi.Json);

        Assert.Equal(HttpStatusCode.Forbidden, profissionais.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, pacientes.StatusCode);
    }

    private async Task<(HttpClient Profissional, Guid Paciente, string Email)> ProfissionalComPacienteAsync()
    {
        var (_, profissional) = await factory.CriarProfissionalComLoginAsync();
        var email = $"paciente-{Guid.NewGuid():N}@exemplo.org";
        var cadastro = await profissional.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Paciente Teste", new DateOnly(1990, 5, 20), null, email, null), TestApi.Json);
        cadastro.EnsureSuccessStatusCode();
        var paciente = (await cadastro.Content.ReadFromJsonAsync<PatientResponse>(TestApi.Json))!.Id;
        return (profissional, paciente, email);
    }

    private static async Task<InvitationIssuedResponse> ConvidarAsync(HttpClient profissional, Guid paciente)
    {
        var resposta = await profissional.PostAsync($"/api/v1/patients/{paciente}/invitations", null);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<InvitationIssuedResponse>(TestApi.Json))!;
    }

    private async Task<(HttpResponseMessage Resposta, HttpClient Portal)> AceitarAsync(string codigo)
    {
        var portal = factory.CreateClient();
        var resposta = await portal.PostAsJsonAsync("/api/v1/patient-invitations/accept",
            new AcceptInvitationRequest(codigo, Senha), TestApi.Json);
        return (resposta, portal);
    }
}
