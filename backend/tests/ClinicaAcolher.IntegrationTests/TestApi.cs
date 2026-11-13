using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.IntegrationTests;

public static class TestApi
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<Guid> CriarPacienteAsync(this HttpClient client, string nome = "Paciente Teste")
    {
        var resposta = await client.PostAsJsonAsync("/api/v1/patients",
            new CreatePatientRequest(nome, new DateOnly(1990, 5, 20), null, null, null), Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PatientResponse>(Json))!.Id;
    }

    public static async Task<Guid> CriarProfissionalAprovadoAsync(this HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync("/api/v1/professionals",
            new CreateProfessionalRequest("Profissional Teste", "Teste", $"prof-{Guid.NewGuid():N}@exemplo.org",
                ProfessionalCategory.Psychoanalyst, null), Json);
        resposta.EnsureSuccessStatusCode();
        var id = (await resposta.Content.ReadFromJsonAsync<ProfessionalResponse>(Json))!.Id;

        (await client.PostAsync($"/api/v1/professionals/{id}/approve", null)).EnsureSuccessStatusCode();
        return id;
    }

    public static async Task<Guid> IniciarVinculoAsync(this HttpClient client, Guid pacienteId, Guid profissionalId)
    {
        var resposta = await client.PostAsJsonAsync("/api/v1/relationships",
            new StartRelationshipRequest(pacienteId, profissionalId), Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<RelationshipResponse>(Json))!.Id;
    }
}