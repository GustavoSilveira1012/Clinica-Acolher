using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Infrastructure.Auditing;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Auditing;

public class AuditTrailTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task RN_SEG_08_registros_ficam_encadeados_e_a_corrente_confere()
    {
        var (primeiro, _) = await RegistrarAsync(3);

        Assert.Null(await VerificarAPartirDeAsync(primeiro));
    }

    [Fact]
    public async Task RN_SEG_08_alterar_um_registro_e_detectado()
    {
        var (primeiro, _) = await RegistrarAsync(3);
        var alvo = primeiro + 1;

        await ExecutarSqlAsync(db => db.Database.ExecuteSqlAsync(
            $"UPDATE audit_log SET \"Action\" = 'Adulterado' WHERE \"Sequence\" = {alvo}"));

        Assert.Equal(alvo, await VerificarAPartirDeAsync(primeiro));
    }

    [Fact]
    public async Task RN_SEG_08_apagar_um_registro_do_meio_e_detectado()
    {
        var (primeiro, _) = await RegistrarAsync(3);
        var alvo = primeiro + 1;

        await ExecutarSqlAsync(db => db.Database.ExecuteSqlAsync(
            $"DELETE FROM audit_log WHERE \"Sequence\" = {alvo}"));

        Assert.Equal(alvo, await VerificarAPartirDeAsync(primeiro));
    }

    [Fact]
    public async Task Gravacoes_simultaneas_nao_bifurcam_a_corrente()
    {
        long antes;
        using (var scope = factory.Services.CreateScope())
            antes = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
                .AuditEntries.MaxAsync(e => (long?)e.Sequence) ?? 0;

        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => RegistrarAsync(1)));

        using var verificacao = factory.Services.CreateScope();
        var db = verificacao.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(antes + 10, await db.AuditEntries.MaxAsync(e => e.Sequence));
        Assert.Null(await VerificarAPartirDeAsync(antes + 1));
    }

    private async Task<(long Primeiro, long Ultimo)> RegistrarAsync(int quantidade)
    {
        using var scope = factory.Services.CreateScope();
        var trilha = scope.ServiceProvider.GetRequiredService<IAuditTrail>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        for (var i = 0; i < quantidade; i++)
            trilha.Record(new AuditEvent("Teste.Registro", "Teste", Guid.NewGuid(), null));
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var ultimo = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .AuditEntries.MaxAsync(e => e.Sequence);
        return (ultimo - quantidade + 1, ultimo);
    }

    private async Task<long?> VerificarAPartirDeAsync(long sequencia)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AuditChainVerifier>()
            .FindFirstBrokenAsync(sequencia, CancellationToken.None);
    }

    private async Task ExecutarSqlAsync(Func<AppDbContext, Task<int>> comando)
    {
        using var scope = factory.Services.CreateScope();
        await comando(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
