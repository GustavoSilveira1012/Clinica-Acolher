using System.Net;
using System.Net.Http.Json;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Application.PatientAccess;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Portal;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.PatientAccess;

public class PortalApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // O relógio dos testes está em 13/11/2026
    private static readonly DateTimeOffset Futura = new(2026, 11, 20, 13, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Passada = new(2026, 11, 6, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Paciente_ve_as_proprias_sessoes_separadas_em_proximas_e_anteriores()
    {
        var c = await CenarioAsync();
        await AgendarAsync(c.Profissional, c.VinculoId, Futura);
        await AgendarAsync(c.Profissional, c.VinculoId, Passada);

        var agenda = await c.Paciente.GetFromJsonAsync<PortalAgendaResponse>("/api/v1/portal/appointments", TestApi.Json);

        var proxima = Assert.Single(agenda!.Upcoming);
        Assert.Equal(Futura, proxima.StartsAt);
        Assert.False(string.IsNullOrWhiteSpace(proxima.ProfessionalName));
        Assert.Equal(Passada, Assert.Single(agenda.Recent).StartsAt);
    }

    [Fact]
    public async Task RN_POR_04_paciente_nao_ve_sessoes_de_outro_paciente()
    {
        var c = await CenarioAsync();
        var outro = await PacienteNoPortalAsync(c.Profissional, c.ProfissionalId);
        await AgendarAsync(c.Profissional, c.VinculoId, Futura);
        await AgendarAsync(c.Profissional, outro.VinculoId, Futura.AddHours(2));

        var agenda = await c.Paciente.GetFromJsonAsync<PortalAgendaResponse>("/api/v1/portal/appointments", TestApi.Json);

        Assert.Equal(Futura, Assert.Single(agenda!.Upcoming).StartsAt);
    }

    [Fact]
    public async Task RN_POR_04_a_equipe_nao_usa_as_rotas_do_portal()
    {
        var c = await CenarioAsync();

        var resposta = await c.Profissional.GetAsync("/api/v1/portal/appointments");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_CAN_01_o_paciente_le_a_politica_pendente_e_aceita()
    {
        var c = await CenarioAsync();
        var politica = await PublicarAsync(c.Profissional, c.ProfissionalId);

        var antes = await PoliticasAsync(c.Paciente);
        var aceite = await c.Paciente.PostAsync($"/api/v1/portal/attendance-policies/{politica.Id}/accept", null);
        var depois = await PoliticasAsync(c.Paciente);

        Assert.False(Assert.Single(antes).Accepted);
        Assert.Equal(HttpStatusCode.OK, aceite.StatusCode);
        Assert.True(Assert.Single(depois).Accepted);
    }

    [Fact]
    public async Task RN_CAN_02_so_a_versao_mais_nova_pode_ser_aceita()
    {
        var c = await CenarioAsync();
        var antiga = await PublicarAsync(c.Profissional, c.ProfissionalId);
        await PublicarAsync(c.Profissional, c.ProfissionalId);

        var resposta = await c.Paciente.PostAsync($"/api/v1/portal/attendance-policies/{antiga.Id}/accept", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Paciente_nao_aceita_politica_de_quem_nao_o_atende()
    {
        var c = await CenarioAsync();
        var (outroId, outroProfissional) = await factory.CriarProfissionalComLoginAsync();
        var politicaAlheia = await PublicarAsync(outroProfissional, outroId);

        var resposta = await c.Paciente.PostAsync($"/api/v1/portal/attendance-policies/{politicaAlheia.Id}/accept", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task RN_CAN_02_nova_versao_fica_pendente_apos_aceite_da_anterior()
    {
        var c = await CenarioAsync();
        var v1 = await PublicarAsync(c.Profissional, c.ProfissionalId);
        (await c.Paciente.PostAsync($"/api/v1/portal/attendance-policies/{v1.Id}/accept", null))
            .EnsureSuccessStatusCode();
        Assert.True(Assert.Single(await PoliticasAsync(c.Paciente)).Accepted);

        var v2 = await PublicarAsync(c.Profissional, c.ProfissionalId);
        var pendente = Assert.Single(await PoliticasAsync(c.Paciente));

        Assert.Equal(v2.Id, pendente.PolicyId);
        Assert.Equal(2, pendente.Version);
        Assert.False(pendente.Accepted);
        Assert.Null(pendente.AcceptedAt);
    }

    [Fact]
    public async Task Aceite_no_portal_registra_auditoria_do_paciente()
    {
        var c = await CenarioAsync();
        var politica = await PublicarAsync(c.Profissional, c.ProfissionalId);
        (await c.Paciente.PostAsync($"/api/v1/portal/attendance-policies/{politica.Id}/accept", null))
            .EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var vinculo = await db.TherapeuticRelationships.SingleAsync(r => r.Id == c.VinculoId);
        var registro = await db.AuditEntries.SingleAsync(e =>
            e.Action == "AttendancePolicy.Accepted" && e.ResourceId == politica.Id);

        Assert.Equal("AttendancePolicy", registro.ResourceType);
        Assert.Equal(vinculo.PatientId, registro.PatientId);
        Assert.NotNull(registro.ActorUserId);
        Assert.NotNull(Assert.Single(await PoliticasAsync(c.Paciente)).AcceptedAt);
    }

    private sealed record Cenario(Guid ProfissionalId, HttpClient Profissional, Guid VinculoId, HttpClient Paciente);

    private async Task<Cenario> CenarioAsync()
    {
        var (profissionalId, profissional) = await factory.CriarProfissionalComLoginAsync();
        var paciente = await PacienteNoPortalAsync(profissional, profissionalId);
        return new Cenario(profissionalId, profissional, paciente.VinculoId, paciente.Client);
    }

    private async Task<(Guid VinculoId, HttpClient Client)> PacienteNoPortalAsync(HttpClient profissional, Guid profissionalId)
    {
        var email = $"paciente-{Guid.NewGuid():N}@exemplo.org";
        var cadastro = await profissional.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Paciente Teste", new DateOnly(1990, 5, 20), null, email, null), TestApi.Json);
        var pacienteId = (await cadastro.Content.ReadFromJsonAsync<PatientResponse>(TestApi.Json))!.Id;

        var vinculos = await profissional.GetFromJsonAsync<List<ActiveRelationshipResponse>>(
            $"/api/v1/professionals/{profissionalId}/relationships/active", TestApi.Json);
        var vinculoId = vinculos!.Single(v => v.PatientId == pacienteId).RelationshipId;

        var convite = await (await profissional.PostAsync($"/api/v1/patients/{pacienteId}/invitations", null))
            .Content.ReadFromJsonAsync<InvitationIssuedResponse>(TestApi.Json);

        var portal = factory.CreateClient();
        (await portal.PostAsJsonAsync("/api/v1/patient-invitations/accept",
            new AcceptInvitationRequest(convite!.Token, "senha-do-paciente-123"), TestApi.Json)).EnsureSuccessStatusCode();

        return (vinculoId, portal);
    }

    private static async Task AgendarAsync(HttpClient profissional, Guid vinculoId, DateTimeOffset inicio) =>
        (await profissional.PostAsJsonAsync("/api/v1/appointments",
            new ScheduleAppointmentRequest(vinculoId, inicio, 50), TestApi.Json)).EnsureSuccessStatusCode();

    private static async Task<AttendancePolicyResponse> PublicarAsync(HttpClient profissional, Guid profissionalId)
    {
        var resposta = await profissional.PostAsJsonAsync($"/api/v1/professionals/{profissionalId}/attendance-policies",
            new PublishAttendancePolicyRequest(24, 100, 100), TestApi.Json);
        return (await resposta.Content.ReadFromJsonAsync<AttendancePolicyResponse>(TestApi.Json))!;
    }

    private static async Task<List<PortalPolicyResponse>> PoliticasAsync(HttpClient paciente) =>
        (await paciente.GetFromJsonAsync<List<PortalPolicyResponse>>("/api/v1/portal/attendance-policies", TestApi.Json))!;
}
