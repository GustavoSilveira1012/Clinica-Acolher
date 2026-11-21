using ClinicaAcolher.Application.PatientAccess;
using ClinicaAcolher.Domain.PatientAccess;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class PatientInvitationRepository(AppDbContext db) : IPatientInvitationRepository
{
    public Task<PatientInvitation?> FindByTokenHashAsync(byte[] tokenHash, CancellationToken ct) =>
        db.PatientInvitations.FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<PatientInvitation>> ListOpenByPatientAsync(Guid patientId, CancellationToken ct) =>
        await db.PatientInvitations
            .Where(i => i.PatientId == patientId && i.UsedAt == null && i.RevokedAt == null)
            .ToListAsync(ct);

    public void Add(PatientInvitation invitation) => db.PatientInvitations.Add(invitation);
}