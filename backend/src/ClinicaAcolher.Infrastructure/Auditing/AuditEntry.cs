using System.Security.Cryptography;
using System.Text;
using ClinicaAcolher.Application.Common;

namespace ClinicaAcolher.Infrastructure.Auditing;

public sealed class AuditEntry
{
    /// <summary>O "registro zero": o primeiro elo aponta para uma impressão digital vazia.</summary>
    public static readonly byte[] GenesisHash = new byte[32];

    public long Sequence { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public Guid ResourceId { get; private set; }
    public Guid? PatientId { get; private set; }
    public byte[] PreviousHash { get; private set; } = [];
    public byte[] Hash { get; private set; } = [];

    private AuditEntry() { }

    internal static AuditEntry Create(
        long sequence, DateTimeOffset now, Guid? actorUserId, AuditEvent auditEvent, byte[] previousHash, byte[] key)
    {
        var entry = new AuditEntry
        {
            Sequence = sequence,
            // O .NET mede o tempo em décimos de microssegundo, e o PostgreSQL guarda só até o microssegundo.
            // Sem arredondar, o horário relido do banco seria diferente, e a impressão digital não bateria.
            OccurredAt = new DateTimeOffset(now.UtcTicks - now.UtcTicks % TimeSpan.TicksPerMillisecond, TimeSpan.Zero),
            ActorUserId = actorUserId,
            Action = auditEvent.Action,
            ResourceType = auditEvent.ResourceType,
            ResourceId = auditEvent.ResourceId,
            PatientId = auditEvent.PatientId,
            PreviousHash = previousHash,
        };
        entry.Hash = entry.ComputeHash(key);
        return entry;
    }

    internal byte[] ComputeHash(byte[] key) =>
        HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(string.Join('|',
            Sequence,
            OccurredAt.UtcDateTime.ToString("O"),
            ActorUserId?.ToString() ?? "-",
            Action,
            ResourceType,
            ResourceId,
            PatientId?.ToString() ?? "-",
            Convert.ToHexString(PreviousHash))));
}