using ClinicaAcolher.Application.ClinicalNotes;
using ClinicaAcolher.Domain.ClinicalNotes;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence.Repositories;

internal sealed class ClinicalNoteRepository(AppDbContext db) : IClinicalNoteRepository
{
    public Task<ClinicalNote?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.ClinicalNotes
            .Include(n => n.Revisions)
            .Include(n => n.Addenda)
            .AsSplitQuery()
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<IReadOnlyList<ClinicalNoteSummary>> ListByPatientAsync(
        Guid professionalId, Guid patientId, CancellationToken ct) =>
        await db.ClinicalNotes
            .AsNoTracking()
            .Where(n => n.ProfessionalId == professionalId && n.PatientId == patientId)
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Select(n => new ClinicalNoteSummary(
                n.Id, n.AppointmentId, n.Status, n.CreatedAt,
                n.Revisions.Max(r => r.SavedAt), n.FinalizedAt,
                n.Revisions.Count, n.Addenda.Count))
            .ToListAsync(ct);

    public void Add(ClinicalNote note) => db.ClinicalNotes.Add(note);
}