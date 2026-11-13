using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.UnitTests.Professionals;

public class ProfessionalTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 12, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RN_PRO_02_psicologo_sem_crp_nao_e_cadastrado()
    {
        var erro = Assert.Throws<DomainException>(() =>
            Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
                ProfessionalCategory.Psychologist, null, Agora));

        Assert.Equal("RN-PRO-02", erro.Rule);
    }

    [Fact]
    public void RN_PRO_04_profissional_nasce_pendente_e_nao_atende()
    {
        var profissional = Professional.Create("Profissional Teste", "Teste", "PROF@Exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);

        Assert.Equal(VerificationStatus.Pending, profissional.Verification);
        Assert.False(profissional.CanAttend);
        Assert.Equal("prof@exemplo.org", profissional.Email);
    }

    [Fact]
    public void RN_PRO_04_aprovacao_libera_atendimento()
    {
        var profissional = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);

        profissional.Approve(Agora);

        Assert.True(profissional.CanAttend);
        Assert.Equal(Agora, profissional.VerifiedAt);
    }

    [Fact]
    public void RN_PRO_04_nao_recusa_profissional_ja_aprovado()
    {
        var profissional = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);
        profissional.Approve(Agora);

        var erro = Assert.Throws<DomainException>(() => profissional.Reject(Agora));

        Assert.Equal("RN-PRO-04", erro.Rule);
    }
}