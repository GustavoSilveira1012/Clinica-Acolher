using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.IntegrationTests.Auth;

public class ProfessionalScopeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset Segunda15h = new(2030, 1, 7, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RN_PER_01_profissional_nao_ve_a_agenda_de_outro()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
            using (profissionalB)
            {
                await AgendarAsync(a.Client, a.VinculoId);

                using var pelaA = await a.Client.GetAsync(AgendaDe(a.ProfissionalId));
                using var pelaB = await profissionalB.GetAsync(AgendaDe(a.ProfissionalId));

                Assert.Equal(HttpStatusCode.OK, pelaA.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, pelaB.StatusCode);
            }
        }
    }

    [Fact]
    public async Task RN_SEG_05_cancelar_sessao_de_outro_e_igual_a_cancelar_sessao_inexistente()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
            using (profissionalB)
            {
                var sessao = await AgendarAsync(a.Client, a.VinculoId);
                var cancelamento = new CancelAppointmentRequest(CancellationSource.Professional, "Tentativa");

                using var alheia = await profissionalB.PostAsJsonAsync(
                    $"/api/v1/appointments/{sessao}/cancel", cancelamento, TestApi.Json);
                using var inexistente = await profissionalB.PostAsJsonAsync(
                    $"/api/v1/appointments/{Guid.NewGuid()}/cancel", cancelamento, TestApi.Json);

                Assert.Equal(HttpStatusCode.NotFound, alheia.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);

                var corpoAlheia = JsonNode.Parse(await alheia.Content.ReadAsStringAsync());
                var corpoInexistente = JsonNode.Parse(await inexistente.Content.ReadAsStringAsync());
                corpoAlheia?.AsObject().Remove("traceId");
                corpoInexistente?.AsObject().Remove("traceId");
                Assert.True(JsonNode.DeepEquals(corpoInexistente, corpoAlheia));
            }
        }
    }

    [Fact]
    public async Task Profissional_nao_ganha_acesso_a_paciente_alheio_criando_um_vinculo()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            var (idB, profissionalB) = await factory.CriarProfissionalComLoginAsync();
            using (profissionalB)
            {
                using var tentativa = await profissionalB.PostAsJsonAsync("/api/v1/relationships",
                    new StartRelationshipRequest(a.PacienteId, idB), TestApi.Json);
                using var depois = await profissionalB.GetAsync($"/api/v1/patients/{a.PacienteId}");

                Assert.Equal(HttpStatusCode.NotFound, tentativa.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, depois.StatusCode);
            }
        }
    }

    [Fact]
    public async Task Profissional_nao_agenda_no_vinculo_de_outro()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
            using (profissionalB)
            {
                using var resposta = await profissionalB.PostAsJsonAsync("/api/v1/appointments",
                    new ScheduleAppointmentRequest(a.VinculoId, Segunda15h, 50), TestApi.Json);

                Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
            }
        }
    }

    [Fact]
    public async Task RN_CAN_01_profissional_nao_aceita_a_politica_no_lugar_do_paciente()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            using var publicacao = await a.Client.PostAsJsonAsync(
                $"/api/v1/professionals/{a.ProfissionalId}/attendance-policies",
                new PublishAttendancePolicyRequest(24, 100, 100), TestApi.Json);
            var politica = (await publicacao.Content.ReadFromJsonAsync<AttendancePolicyResponse>(TestApi.Json))!;

            using var aceite = await a.Client.PostAsJsonAsync(
                $"/api/v1/attendance-policies/{politica.Id}/acceptances",
                new AcceptPolicyRequest(a.PacienteId), TestApi.Json);

            Assert.Equal(HttpStatusCode.Created, publicacao.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, aceite.StatusCode);
        }
    }

    [Fact]
    public async Task Profissional_nao_encerra_a_serie_de_outro()
    {
        var a = await ProfissionalComPacienteAsync();
        using (a.Client)
        {
            var (_, profissionalB) = await factory.CriarProfissionalComLoginAsync();
            using (profissionalB)
            {
                using var criacao = await a.Client.PostAsJsonAsync("/api/v1/series", new CreateSeriesRequest(
                    a.VinculoId, DayOfWeek.Monday, new TimeOnly(9, 0), 50, "America/Sao_Paulo",
                    new DateOnly(2026, 11, 23)), TestApi.Json);
                criacao.EnsureSuccessStatusCode();
                var serie = (await criacao.Content.ReadFromJsonAsync<SeriesResponse>(TestApi.Json))!;

                using var resposta = await profissionalB.PostAsJsonAsync($"/api/v1/series/{serie.Id}/end",
                    new EndSeriesRequest(new DateOnly(2026, 11, 30)), TestApi.Json);

                Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
            }
        }
    }

    private sealed record Cenario(Guid ProfissionalId, HttpClient Client, Guid PacienteId, Guid VinculoId);

    private async Task<Cenario> ProfissionalComPacienteAsync()
    {
        var (profissionalId, client) = await factory.CriarProfissionalComLoginAsync();

        var cadastro = await client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest("Paciente Teste", new DateOnly(1990, 5, 20), null, null, null), TestApi.Json);
        cadastro.EnsureSuccessStatusCode();
        var pacienteId = (await cadastro.Content.ReadFromJsonAsync<PatientResponse>(TestApi.Json))!.Id;

        var vinculos = await client.GetFromJsonAsync<List<ActiveRelationshipResponse>>(
            $"/api/v1/professionals/{profissionalId}/relationships/active", TestApi.Json);

        return new Cenario(profissionalId, client, pacienteId, vinculos!.Single().RelationshipId);
    }

    private static async Task<Guid> AgendarAsync(HttpClient client, Guid vinculoId)
    {
        using var resposta = await client.PostAsJsonAsync("/api/v1/appointments",
            new ScheduleAppointmentRequest(vinculoId, Segunda15h, 50), TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<AppointmentResponse>(TestApi.Json))!.Id;
    }

    private static string AgendaDe(Guid profissionalId) =>
        $"/api/v1/professionals/{profissionalId}/agenda?from=2030-01-07T00:00:00Z&to=2030-01-08T00:00:00Z";
}
