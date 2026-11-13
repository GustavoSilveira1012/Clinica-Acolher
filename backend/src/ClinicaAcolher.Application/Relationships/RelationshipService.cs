using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Application.Relationships;

public sealed class RelationshipService(
    IPatientRepository patients,
    IProfessionalRepository professionals,
    IRelationshipRepository relationships,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<RelationshipResponse> StartAsync(StartRelationshipRequest r, CancellationToken ct)
    {
        var patient = await patients.GetByIdAsync(r.PatientId, ct)
            ?? throw new NotFoundException("Paciente não encontrado.");

        var professional = await professionals.GetByIdAsync(r.ProfessionalId, ct)
            ?? throw new NotFoundException("Profissional não encontrado.");

        if (await relationships.HasOpenAsync(patient.Id, professional.Id, ct))
            throw new DomainException("RN-VIN-02", "Este paciente já tem um vínculo em aberto com este profissional.");

        var relationship = TherapeuticRelationship.Start(patient, professional, clock.GetUtcNow());
        relationships.Add(relationship);
        await unitOfWork.SaveChangesAsync(ct);

        return RelationshipResponse.From(relationship);
    }

    public async Task<RelationshipResponse> GetByIdAsync(Guid id, CancellationToken ct) =>
        RelationshipResponse.From(await LoadAsync(id, ct));

    public async Task<IReadOnlyList<RelationshipResponse>> ListByPatientAsync(Guid patientId, CancellationToken ct)
    {
        var list = await relationships.ListByPatientAsync(patientId, ct);
        return list.Select(RelationshipResponse.From).ToList();
    }

    public Task<RelationshipResponse> SuspendAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (r, _) => r.Suspend(), ct);

    public Task<RelationshipResponse> ResumeAsync(Guid id, CancellationToken ct) =>
        ChangeAsync(id, (r, _) => r.Resume(), ct);

    public Task<RelationshipResponse> EndAsync(Guid id, EndRelationshipRequest request, CancellationToken ct) =>
        ChangeAsync(id, (r, now) => r.End(request.Reason, now), ct);

    private async Task<RelationshipResponse> ChangeAsync(
        Guid id, Action<TherapeuticRelationship, DateTimeOffset> change, CancellationToken ct)
    {
        var relationship = await LoadAsync(id, ct);
        change(relationship, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return RelationshipResponse.From(relationship);
    }

    private async Task<TherapeuticRelationship> LoadAsync(Guid id, CancellationToken ct) =>
        await relationships.GetByIdAsync(id, ct) ?? throw new NotFoundException("Vínculo não encontrado.");
}