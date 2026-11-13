using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class RelationshipRepository(AppDbContext db) : IRelationshipRepository
{
    public Task<TherapeuticRelationship?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.TherapeuticRelationships.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> HasOpenAsync(Guid patientId, Guid professionalId, CancellationToken ct) =>
        db.TherapeuticRelationships.AnyAsync(r =>
            r.PatientId == patientId &&
            r.ProfessionalId == professionalId &&
            r.Status != RelationshipStatus.Ended, ct);

    public async Task<IReadOnlyList<TherapeuticRelationship>> ListByPatientAsync(Guid patientId, CancellationToken ct) =>
        await db.TherapeuticRelationships
            .AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);

    public void Add(TherapeuticRelationship relationship) => db.TherapeuticRelationships.Add(relationship);
}