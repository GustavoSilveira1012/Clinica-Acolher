using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Application.Common;

namespace ClinicaAcolher.Application.Patients;

public sealed class PatientService(IPatientRepository patients, TimeProvider clock)
{
    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken ct)
    {
        var patient = Patient.Create(
            request.FullName, request.BirthDate, clock.GetUtcNow(),
            request.SocialName, request.Email, request.Phone);

            patients.Add(patient);
            await patients.SaveChangesAsync(ct);

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
}
