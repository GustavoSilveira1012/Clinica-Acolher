using System.Security.Cryptography;
using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicaAcolher.Infrastructure.Auditing;

public sealed class AuditChainVerifier(AppDbContext db, IOptions<AuditOptions> options)
{
    private readonly byte[] _key = Convert.FromBase64String(options.Value.HmacKey);

    /// <summary>
    /// Percorre a corrente a partir de um registro. Devolve o número do primeiro elo
    /// adulterado ou ausente, ou nulo se a corrente estiver íntegra.
    /// </summary>
    public async Task<long?> FindFirstBrokenAsync(long fromSequence, CancellationToken ct)
    {
        var previous = fromSequence <= 1
            ? AuditEntry.GenesisHash
            : (await db.AuditEntries.AsNoTracking().SingleOrDefaultAsync(e => e.Sequence == fromSequence - 1, ct))?.Hash;

        if (previous is null)
            return fromSequence - 1;

        var expected = fromSequence;
        var entries = db.AuditEntries.AsNoTracking()
            .Where(e => e.Sequence >= fromSequence)
            .OrderBy(e => e.Sequence)
            .AsAsyncEnumerable();

        await foreach (var entry in entries.WithCancellation(ct))
        {
            var intact = entry.Sequence == expected
                && CryptographicOperations.FixedTimeEquals(entry.PreviousHash, previous)
                && CryptographicOperations.FixedTimeEquals(entry.Hash, entry.ComputeHash(_key));

            if (!intact)
                return expected;

            previous = entry.Hash;
            expected++;
        }

        return null;
    }
}