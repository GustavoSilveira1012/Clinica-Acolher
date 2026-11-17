using ClinicaAcolher.Domain.ClinicalNotes;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.ClinicalNotes;

public class ClinicalNotePersistenceTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Anotacao_com_versoes_e_adendo_e_gravada_e_reconstruida()
    {
        var client = factory.AdminClient;
        var vinculoId = await client.IniciarVinculoAsync(
            await client.CriarPacienteAsync(), await client.CriarProfissionalAprovadoAsync());
        var agora = factory.Clock.GetUtcNow();
        Guid notaId;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var vinculo = await db.TherapeuticRelationships.SingleAsync(r => r.Id == vinculoId);

            var nota = ClinicalNote.Start(vinculo, null, [1, 2, 3], agora);
            nota.SaveDraft([4, 5, 6], agora);
            nota.FinalizeNote(agora);
            nota.AddAddendum([7, 8], agora);

            db.ClinicalNotes.Add(nota);
            await db.SaveChangesAsync();
            notaId = nota.Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var lida = await db.ClinicalNotes
                .Include(n => n.Revisions)
                .Include(n => n.Addenda)
                .SingleAsync(n => n.Id == notaId);

            Assert.Equal(ClinicalNoteStatus.Finalized, lida.Status);
            Assert.Equal(2, lida.Revisions.Count);
            Assert.Equal(new byte[] { 4, 5, 6 }, lida.CurrentRevision.EncryptedContent);
            Assert.Equal(new byte[] { 7, 8 }, Assert.Single(lida.Addenda).EncryptedContent);
        }
    }
}
