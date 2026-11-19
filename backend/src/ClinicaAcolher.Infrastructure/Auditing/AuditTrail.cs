using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicaAcolher.Infrastructure.Auditing;

public sealed class AuditTrail(
    AppDbContext db, ICurrentUser user, TimeProvider clock, IOptions<AuditOptions> options) : IAuditTrail
{
    // Um número qualquer, que identifica a "tranca" da trilha de auditoria no PostgreSQL
    private const long ChainLockId = 0x41756469;

    private readonly byte[] _key = Convert.FromBase64String(options.Value.HmacKey);
    private readonly List<AuditEvent> _pending = [];

    public bool HasPending => _pending.Count > 0;

    public void Record(AuditEvent auditEvent) => _pending.Add(auditEvent);

    /// <summary>Encadeia os registros pendentes. Precisa rodar dentro de uma transação.</summary>
    internal async Task ChainPendingAsync(CancellationToken ct)
    {
        // Só uma transação por vez acrescenta elos à corrente; as outras esperam a vez
        await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ChainLockId})", ct);

        var last = await db.AuditEntries.AsNoTracking().OrderByDescending(e => e.Sequence).FirstOrDefaultAsync(ct);
        var sequence = last?.Sequence ?? 0;
        var previousHash = last?.Hash ?? AuditEntry.GenesisHash;
        var now = clock.GetUtcNow();

        foreach (var auditEvent in _pending)
        {
            var entry = AuditEntry.Create(++sequence, now, user.UserId, auditEvent, previousHash, _key);
            db.AuditEntries.Add(entry);
            previousHash = entry.Hash;
        }

        _pending.Clear();
    }
}