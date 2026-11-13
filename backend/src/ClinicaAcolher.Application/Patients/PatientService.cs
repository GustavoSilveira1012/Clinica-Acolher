using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Application.Common;

namespace ClinicaAcolher.Application.Patients;

public sealed class PatientService(IPatientRepository patients, IUnitOfWork unitOfWork, TimeProvider clock)
{
    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken ct)
    {
        var patient = Patient.Create(
            request.FullName, request.BirthDate, clock.GetUtcNow(),
            request.SocialName, request.Email, request.Phone);

            patients.Add(patient);
            await unitOfWork.SaveChangesAsync(ct);

        return PatientResponse.From(patient);
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var patient = await patients.GetByIdAsync(id, ct);
        return patient is null ? null : PatientResponse.From(patient);
    }

    public async Task<PagedResult<PatientListItem>> SearchAsync(PatientSearchRequest request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, total) = await patients.SearchAsync(
            request.Search, request.Active, (page - 1) * pageSize, pageSize, ct);

        return new PagedResult<PatientListItem>(
            items.Select(PatientListItem.From).ToList(), page, pageSize, total
        );
    }

public Task<PatientResponse?> UpdateAsync(Guid id, UpdatePatientRequest r, CancellationToken ct) =>
    ChangeAsync(id, (p, now) => p.UpdateData(r.FullName, r.BirthDate, now, r.SocialName, r.Email, r.Phone), ct);

public Task<PatientResponse?> DeactivateAsync(Guid id, DeactivatePatientRequest r, CancellationToken ct) =>
    ChangeAsync(id, (p, now) => p.Deactivate(r.Reason, now), ct);

public Task<PatientResponse?> ReactivateAsync(Guid id, CancellationToken ct) =>
    ChangeAsync(id, (p, now) => p.Reactivate(now), ct);

private async Task<PatientResponse?> ChangeAsync(
    Guid id, Action<Patient, DateTimeOffset> change, CancellationToken ct)
{
    var patient = await patients.GetByIdAsync(id, ct);
    if (patient is null)
        return null;

    change(patient, clock.GetUtcNow());
    await unitOfWork.SaveChangesAsync(ct);

    return PatientResponse.From(patient);
}
}
