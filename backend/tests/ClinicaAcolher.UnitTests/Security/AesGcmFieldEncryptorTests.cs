using System.Security.Cryptography;
using System.Text;
using ClinicaAcolher.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace ClinicaAcolher.UnitTests.Security;

public class AesGcmFieldEncryptorTests
{
    private const string Contexto = "clinical-note:0001";
    private static readonly string ChaveA = NovaChave();
    private static readonly string ChaveB = NovaChave();

    [Fact]
    public void RN_PRT_02_cifra_e_decifra_de_volta_o_mesmo_texto()
    {
        var cifrador = Cifrador("a", ("a", ChaveA));
        const string texto = "Sessão 12: relato fictício, com acentuação e emoji 🌱.";

        Assert.Equal(texto, cifrador.Decrypt(cifrador.Encrypt(texto, Contexto), Contexto));
    }

    [Fact]
    public void O_texto_original_nao_aparece_no_dado_cifrado()
    {
        var cifrado = Cifrador("a", ("a", ChaveA)).Encrypt("palavra-reconhecivel", Contexto);

        Assert.DoesNotContain("palavra-reconhecivel", Encoding.UTF8.GetString(cifrado));
    }

    [Fact]
    public void O_mesmo_texto_cifrado_duas_vezes_gera_resultados_diferentes()
    {
        var cifrador = Cifrador("a", ("a", ChaveA));

        Assert.NotEqual(cifrador.Encrypt("igual", Contexto), cifrador.Encrypt("igual", Contexto));
    }

    [Fact]
    public void Alterar_um_unico_byte_impede_a_decifragem()
    {
        var cifrador = Cifrador("a", ("a", ChaveA));
        var cifrado = cifrador.Encrypt("texto original", Contexto);

        cifrado[^1] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => cifrador.Decrypt(cifrado, Contexto));
    }

    [Fact]
    public void Dado_cifrado_copiado_para_outro_registro_nao_abre()
    {
        var cifrador = Cifrador("a", ("a", ChaveA));
        var cifrado = cifrador.Encrypt("anotação do paciente 1", "clinical-note:0001");

        Assert.ThrowsAny<CryptographicException>(() => cifrador.Decrypt(cifrado, "clinical-note:0002"));
    }

    [Fact]
    public void Depois_da_rotacao_os_dados_antigos_continuam_abrindo()
    {
        var antigo = Cifrador("a", ("a", ChaveA)).Encrypt("anotação antiga", Contexto);
        var depoisDaRotacao = Cifrador("b", ("a", ChaveA), ("b", ChaveB));

        Assert.Equal("anotação antiga", depoisDaRotacao.Decrypt(antigo, Contexto));
    }

    [Fact]
    public void Sem_a_chave_que_fechou_o_envelope_nao_ha_como_abrir()
    {
        var cifrado = Cifrador("a", ("a", ChaveA)).Encrypt("anotação", Contexto);
        var semAChaveA = Cifrador("b", ("b", ChaveB));

        Assert.Throws<InvalidOperationException>(() => semAChaveA.Decrypt(cifrado, Contexto));
    }

    private static AesGcmFieldEncryptor Cifrador(string ativa, params (string Id, string Chave)[] chaves) =>
        new(Options.Create(new EncryptionOptions
        {
            ActiveKeyId = ativa,
            Keys = chaves.ToDictionary(c => c.Id, c => c.Chave),
        }));

    private static string NovaChave() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
