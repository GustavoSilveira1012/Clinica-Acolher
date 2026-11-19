using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClinicaAcolher.Application.ClinicalNotes;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.ClinicalNotes;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.ClinicalNotes;

public class ClinicalNotesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task RN_PRT_03_04_rascunho_versoes_finalizacao_e_adendo()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Primeira versão do rascunho.");

            using var revisao = await c.Client.PostAsJsonAsync($"/api/v1/clinical-notes/{nota.Id}/revisions",
                new ClinicalNoteContentRequest("Segunda versão, revisada."), TestApi.Json);
            revisao.EnsureSuccessStatusCode();
            using var finalizacao = await c.Client.PostAsync($"/api/v1/clinical-notes/{nota.Id}/finalize", null);
            finalizacao.EnsureSuccessStatusCode();
            using var adendo = await c.Client.PostAsJsonAsync($"/api/v1/clinical-notes/{nota.Id}/addenda",
                new ClinicalNoteContentRequest("Adendo: correção de uma data."), TestApi.Json);
            adendo.EnsureSuccessStatusCode();
            var lida = await LerAsync(c.Client, nota.Id);

            Assert.Equal(ClinicalNoteStatus.Finalized, lida.Status);
            Assert.Equal("Segunda versão, revisada.", lida.Content);
            Assert.Equal(2, lida.RevisionNumber);
            Assert.Equal("Adendo: correção de uma data.", Assert.Single(lida.Addenda).Content);
        }
    }

    [Fact]
    public async Task RN_PRT_02_o_texto_fica_cifrado_no_banco()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            const string marcador = "texto-que-nao-pode-aparecer-no-banco";
            var nota = await CriarAsync(c, marcador);

            using var scope = factory.Services.CreateScope();
            var gravado = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
                .Set<ClinicalNoteRevision>()
                .Where(r => r.NoteId == nota.Id)
                .Select(r => r.EncryptedContent)
                .SingleAsync();

            Assert.DoesNotContain(marcador, Encoding.UTF8.GetString(gravado));
        }
    }

    [Fact]
    public async Task RN_PER_01_outro_profissional_recebe_o_mesmo_404_de_uma_anotacao_inexistente()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Conteúdo sigiloso.");
            var (_, outroProfissional) = await factory.CriarProfissionalComLoginAsync();
            using (outroProfissional)
            {
                using var alheia = await outroProfissional.GetAsync($"/api/v1/clinical-notes/{nota.Id}");
                using var inexistente = await outroProfissional.GetAsync(
                    $"/api/v1/clinical-notes/{Guid.NewGuid()}");

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
    public async Task RN_PRT_01_a_administracao_da_clinica_nao_acessa_anotacoes()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Conteúdo sigiloso.");

            using var leitura = await factory.AdminClient.GetAsync($"/api/v1/clinical-notes/{nota.Id}");
            using var lista = await factory.AdminClient.GetAsync(
                $"/api/v1/patients/{c.PacienteId}/clinical-notes");

            Assert.Equal(HttpStatusCode.Forbidden, leitura.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, lista.StatusCode);
        }
    }

    [Fact]
    public async Task RN_PRT_05_cada_leitura_fica_registrada_na_auditoria()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Conteúdo sigiloso.");
            var antes = await ContarRegistrosAsync(nota.Id, ClinicalNoteAudit.Read);

            await LerAsync(c.Client, nota.Id);
            await LerAsync(c.Client, nota.Id);

            Assert.Equal(antes + 2, await ContarRegistrosAsync(nota.Id, ClinicalNoteAudit.Read));
        }
    }

    [Fact]
    public async Task RN_PRT_03_anotacao_finalizada_recusa_versao_nova()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Texto final.");
            using var finalizacao = await c.Client.PostAsync($"/api/v1/clinical-notes/{nota.Id}/finalize", null);
            finalizacao.EnsureSuccessStatusCode();

            using var resposta = await c.Client.PostAsJsonAsync($"/api/v1/clinical-notes/{nota.Id}/revisions",
                new ClinicalNoteContentRequest("Tentando alterar."), TestApi.Json);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        }
    }

    [Fact]
    public async Task RN_PER_04_com_o_vinculo_encerrado_le_mas_nao_escreve()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Rascunho antes do encerramento.");
            using var encerramento = await c.Client.PostAsJsonAsync(
                $"/api/v1/relationships/{c.VinculoId}/end",
                new EndRelationshipRequest("Encerrou o acompanhamento"), TestApi.Json);
            encerramento.EnsureSuccessStatusCode();

            using var leitura = await c.Client.GetAsync($"/api/v1/clinical-notes/{nota.Id}");
            using var escrita = await c.Client.PostAsJsonAsync($"/api/v1/clinical-notes/{nota.Id}/revisions",
                new ClinicalNoteContentRequest("Depois do encerramento."), TestApi.Json);

            Assert.Equal(HttpStatusCode.OK, leitura.StatusCode);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, escrita.StatusCode);
        }
    }

    [Fact]
    public async Task Respostas_com_conteudo_clinico_nao_ficam_em_cache()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            var nota = await CriarAsync(c, "Conteúdo sigiloso.");

            using var resposta = await c.Client.GetAsync($"/api/v1/clinical-notes/{nota.Id}");

            Assert.True(resposta.Headers.CacheControl?.NoStore);
        }
    }

    [Fact]
    public async Task A_lista_traz_so_dados_de_controle_sem_o_texto()
    {
        var c = await factory.CriarProfissionalComPacienteAsync();
        using (c.Client)
        {
            await CriarAsync(c, "Conteúdo sigiloso.");

            var lista = await c.Client.GetFromJsonAsync<JsonElement>(
                $"/api/v1/patients/{c.PacienteId}/clinical-notes");

            var item = Assert.Single(lista.EnumerateArray());
            Assert.False(item.TryGetProperty("content", out _));
            Assert.Equal(1, item.GetProperty("revisionCount").GetInt32());
        }
    }

    private static async Task<ClinicalNoteResponse> CriarAsync(ProfissionalComPaciente c, string texto)
    {
        using var resposta = await c.Client.PostAsJsonAsync("/api/v1/clinical-notes",
            new CreateClinicalNoteRequest(c.VinculoId, null, texto), TestApi.Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ClinicalNoteResponse>(TestApi.Json))!;
    }

    private static async Task<ClinicalNoteResponse> LerAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<ClinicalNoteResponse>($"/api/v1/clinical-notes/{id}", TestApi.Json))!;

    private async Task<int> ContarRegistrosAsync(Guid notaId, string acao)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .AuditEntries.CountAsync(e => e.ResourceId == notaId && e.Action == acao);
    }
}
