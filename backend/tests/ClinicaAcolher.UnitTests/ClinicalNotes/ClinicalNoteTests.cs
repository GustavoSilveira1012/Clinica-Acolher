using ClinicaAcolher.Domain.ClinicalNotes;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;
using static ClinicaAcolher.UnitTests.Cenarios;

namespace ClinicaAcolher.UnitTests.ClinicalNotes;

public class ClinicalNoteTests
{
    // Para o domínio, o conteúdo é uma caixa lacrada: qualquer sequência de bytes serve no teste
    private static readonly byte[] Conteudo1 = [1, 2, 3];
    private static readonly byte[] Conteudo2 = [4, 5, 6];

    private static TherapeuticRelationship Vinculo() =>
        TherapeuticRelationship.Start(Paciente(), ProfissionalAprovado(), Agora);

    [Fact]
    public void RN_PRT_01_nasce_como_rascunho_com_a_primeira_versao()
    {
        var vinculo = Vinculo();

        var nota = ClinicalNote.Start(vinculo, null, Conteudo1, Agora);

        Assert.Equal(ClinicalNoteStatus.Draft, nota.Status);
        Assert.Equal(vinculo.PatientId, nota.PatientId);
        Assert.Equal(1, nota.CurrentRevision.Number);
    }

    [Fact]
    public void RN_PRT_04_cada_salvamento_cria_uma_versao_sem_apagar_as_anteriores()
    {
        var nota = ClinicalNote.Start(Vinculo(), null, Conteudo1, Agora);

        nota.SaveDraft(Conteudo2, Agora.AddMinutes(10));

        Assert.Equal(2, nota.Revisions.Count);
        Assert.Equal(Conteudo1, nota.Revisions.Single(r => r.Number == 1).EncryptedContent);
        Assert.Equal(Conteudo2, nota.CurrentRevision.EncryptedContent);
    }

    [Fact]
    public void RN_PRT_03_anotacao_finalizada_nao_aceita_edicao()
    {
        var nota = ClinicalNote.Start(Vinculo(), null, Conteudo1, Agora);
        nota.FinalizeNote(Agora);

        var erro = Assert.Throws<DomainException>(() => nota.SaveDraft(Conteudo2, Agora));

        Assert.Equal("RN-PRT-03", erro.Rule);
        Assert.Single(nota.Revisions);
    }

    [Fact]
    public void RN_PRT_03_correcao_depois_de_finalizar_e_por_adendo()
    {
        var nota = ClinicalNote.Start(Vinculo(), null, Conteudo1, Agora);
        nota.FinalizeNote(Agora);

        nota.AddAddendum(Conteudo2, Agora.AddDays(1));

        Assert.Single(nota.Addenda);
        Assert.Equal(Conteudo1, nota.CurrentRevision.EncryptedContent);
    }

    [Fact]
    public void Cada_versao_e_cifrada_com_um_contexto_proprio()
    {
        var contextos = new List<string>();
        byte[] Selar(string contexto)
        {
            contextos.Add(contexto);
            return Conteudo1;
        }

        var nota = ClinicalNote.Start(Vinculo(), null, Selar, Agora);
        nota.SaveDraft(Selar, Agora.AddMinutes(1));
        nota.FinalizeNote(Agora.AddMinutes(2));
        nota.AddAddendum(Selar, Agora.AddMinutes(3));

        Assert.Equal(
            [
                ClinicalNote.RevisionContext(nota.Id, 1),
                ClinicalNote.RevisionContext(nota.Id, 2),
                ClinicalNote.AddendumContext(nota.Id, 1),
            ],
            contextos);
    }

    [Fact]
    public void Adendo_so_existe_em_anotacao_finalizada()
    {
        var nota = ClinicalNote.Start(Vinculo(), null, Conteudo1, Agora);

        var erro = Assert.Throws<DomainException>(() => nota.AddAddendum(Conteudo2, Agora));

        Assert.Equal("RN-PRT-03", erro.Rule);
    }

    [Fact]
    public void RN_PER_04_vinculo_encerrado_nao_recebe_anotacao_nova()
    {
        var vinculo = Vinculo();
        vinculo.End("Encerrou o acompanhamento", Agora);

        var erro = Assert.Throws<DomainException>(() => ClinicalNote.Start(vinculo, null, Conteudo1, Agora));

        Assert.Equal("RN-PER-04", erro.Rule);
    }

    [Fact]
    public void Alterar_o_array_original_depois_nao_muda_a_versao_guardada()
    {
        var conteudo = new byte[] { 9, 9, 9 };
        var nota = ClinicalNote.Start(Vinculo(), null, conteudo, Agora);

        conteudo[0] = 0;

        Assert.Equal(9, nota.CurrentRevision.EncryptedContent[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RN_PRT_10_rejeita_texto_vazio(string texto)
    {
        Assert.Equal("RN-PRT-10", Assert.Throws<DomainException>(() => ClinicalNote.EnsureValidContent(texto)).Rule);
    }

    [Fact]
    public void RN_PRT_10_rejeita_texto_longo_demais()
    {
        var texto = new string('a', ClinicalNote.MaxContentLength + 1);

        Assert.Equal("RN-PRT-10", Assert.Throws<DomainException>(() => ClinicalNote.EnsureValidContent(texto)).Rule);
    }
}
