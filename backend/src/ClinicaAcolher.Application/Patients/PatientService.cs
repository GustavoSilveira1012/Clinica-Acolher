using ClinicaAcolher.Domain.Patients;

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
}
