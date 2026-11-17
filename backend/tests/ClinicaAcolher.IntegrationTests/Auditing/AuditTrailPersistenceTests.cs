using System.Security.Claims;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Infrastructure.Auditing;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAcolher.IntegrationTests.Auditing;

public class AuditTrailPersistenceTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Grava_o_responsavel_e_encadeia_a_entrada_na_transacao()
    {
        var actorId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, actorId.ToString())], "test")),
            };
            scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;

            var auditTrail = scope.ServiceProvider.GetRequiredService<IAuditTrail>();
            auditTrail.Record(new AuditEvent("clinical-note.created", "ClinicalNote", resourceId, null));
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entry = await db.AuditEntries.SingleAsync(e => e.ResourceId == resourceId);

            Assert.Equal(actorId, entry.ActorUserId);
            Assert.Equal(AuditEntry.GenesisHash, entry.PreviousHash);
            Assert.Equal(32, entry.Hash.Length);
        }
    }
}
