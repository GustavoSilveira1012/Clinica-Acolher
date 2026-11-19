using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Infrastructure.Identity;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Patients;

public class PatientAccessTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "senha-de-teste-123";
    private static readonly DateOnly BirthDate = new(1990, 5, 20);

    [Fact]
    public async Task Profissional_cria_paciente_com_vinculo_e_pode_editar()
    {
        var (professionalId, client) = await ProfessionalClientAsync();
        using (client)
        {
            var patientId = await client.CriarPacienteAsync();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var relationship = await db.TherapeuticRelationships.SingleAsync(r => r.PatientId == patientId);
            Assert.Equal(professionalId, relationship.ProfessionalId);

            using var edit = await client.PutAsJsonAsync($"/api/v1/patients/{patientId}",
                new UpdatePatientRequest("Nome Editado", BirthDate, null, null, null));
            Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
            var patient = await client.GetFromJsonAsync<PatientResponse>($"/api/v1/patients/{patientId}");
            Assert.Equal("Nome Editado", patient!.FullName);
        }
    }

    [Fact]
    public async Task Busca_e_total_respeitam_escopo_incluindo_vinculo_encerrado()
    {
        var prefix = $"Escopo {Guid.NewGuid():N}";
        var (_, first) = await ProfessionalClientAsync();
        var (_, second) = await ProfessionalClientAsync();
        using (first)
        using (second)
        {
            var endedPatient = await first.CriarPacienteAsync(prefix + " A");
            var activePatient = await first.CriarPacienteAsync(prefix + " B");
            await second.CriarPacienteAsync(prefix + " C");
            await factory.AdminClient.CriarPacienteAsync(prefix + " D");
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var relationship = await db.TherapeuticRelationships.SingleAsync(r => r.PatientId == endedPatient);
                relationship.End("Fim do acompanhamento fictício", factory.Clock.GetUtcNow());
                await db.SaveChangesAsync();
            }

            var page = await SearchAsync(first, prefix, page: 1);
            Assert.Equal(2, page.TotalCount);
            Assert.Equal(endedPatient, Assert.Single(page.Items).id);
            Assert.Equal(activePatient, Assert.Single((await SearchAsync(first, prefix, page: 2)).Items).id);
            Assert.Equal(4, (await SearchAsync(factory.AdminClient, prefix)).TotalCount);
            using var history = await first.GetAsync($"/api/v1/patients/{endedPatient}");
            Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        }
    }

    [Theory]
    [InlineData("get")]
    [InlineData("update")]
    [InlineData("deactivate")]
    [InlineData("reactivate")]
    public async Task Sem_vinculo_retorna_o_mesmo_404_de_id_inexistente(string action)
    {
        var (_, client) = await ProfessionalClientAsync();
        using (client)
        {
            var id = await factory.AdminClient.CriarPacienteAsync("Paciente protegido");
            using var denied = await SendAsync(client, action, id);
            using var missing = await SendAsync(client, action, Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
            var deniedBody = await denied.Content.ReadFromJsonAsync<JsonObject>();
            var missingBody = await missing.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(deniedBody);
            Assert.NotNull(missingBody);
            deniedBody.Remove("traceId");
            missingBody.Remove("traceId");
            Assert.True(JsonNode.DeepEquals(deniedBody, missingBody));

            var unchanged = await factory.AdminClient.GetFromJsonAsync<PatientResponse>($"/api/v1/patients/{id}");
            Assert.Equal("Paciente protegido", unchanged!.FullName);
            Assert.True(unchanged.Active);
        }
    }

    [Theory]
    [InlineData(false, false, "RN-VIN-01")]
    [InlineData(true, true, "RN-PAC-03")]
    public async Task Falha_ao_iniciar_vinculo_nao_grava_paciente(bool approved, bool minor, string rule)
    {
        var (professionalId, client) = await ProfessionalClientAsync(approved);
        using (client)
        {
            var email = $"pac-{Guid.NewGuid():N}@exemplo.org";
            using var response = await client.PostAsJsonAsync("/api/v1/patients",
                new CreatePatientRequest("Paciente Fictício", minor ? new DateOnly(2020, 1, 1) : BirthDate,
                    null, email, null));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(rule, problem.GetProperty("code").GetString());
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.False(await db.Patients.AnyAsync(p => p.Email == email));
            Assert.False(await db.TherapeuticRelationships.AnyAsync(r => r.ProfessionalId == professionalId));
        }
    }

    [Fact]
    public async Task Perfil_sem_escopo_nao_lista_nem_cadastra_pacientes()
    {
        var email = $"usuario-{Guid.NewGuid():N}@exemplo.org";
        await factory.CreateUserAsync(email, Password, Roles.Patient);
        using var client = await factory.CreateLoggedInClientAsync(email, Password);
        using var search = await client.PostAsJsonAsync("/api/v1/patients/search", new PatientSearchRequest(null, null));
        using var create = await client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Sem permissão", BirthDate, null, email, null));
        Assert.Equal(HttpStatusCode.Forbidden, search.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<AppDbContext>().Patients.AnyAsync(p => p.Email == email));
    }

    private async Task<(Guid Id, HttpClient Client)> ProfessionalClientAsync(bool approved = true)
    {
        if (approved)
            return await factory.CriarProfissionalComLoginAsync();

        var email = $"prof-{Guid.NewGuid():N}@exemplo.org";
        using var response = await factory.AdminClient.PostAsJsonAsync("/api/v1/professionals",
            new CreateProfessionalRequest("Profissional Fictício", "Teste", email,
                ProfessionalCategory.Psychoanalyst, null), TestApi.Json);
        response.EnsureSuccessStatusCode();
        var professional = (await response.Content.ReadFromJsonAsync<ProfessionalResponse>(TestApi.Json))!;
        return (professional.Id, await factory.CreateStaffClientAsync(email, Password, Roles.Professional, professional.Id));
    }

    private static async Task<PagedResult<PatientListItem>> SearchAsync(HttpClient client, string prefix, int page = 1)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/patients/search",
            new PatientSearchRequest(prefix, true, page, 1));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PagedResult<PatientListItem>>())!;
    }

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, string action, Guid id) => action switch
    {
        "get" => client.GetAsync($"/api/v1/patients/{id}"),
        "update" => client.PutAsJsonAsync($"/api/v1/patients/{id}",
            new UpdatePatientRequest("Alteração proibida", BirthDate, null, null, null)),
        "deactivate" => client.PostAsJsonAsync($"/api/v1/patients/{id}/deactivate", new DeactivatePatientRequest("Teste")),
        "reactivate" => client.PostAsync($"/api/v1/patients/{id}/reactivate", null),
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
}
