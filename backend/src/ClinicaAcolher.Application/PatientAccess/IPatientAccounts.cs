namespace ClinicaAcolher.Application.PatientAccess;

public interface IPatientAccounts
{
    Task<bool> HasAccountAsync(Guid patientId, CancellationToken ct);

    /// <summary>Cria a conta com o papel de paciente e devolve o Id do usuário.</summary>
    Task<Guid> CreateAsync(Guid patientId, string email, string password, CancellationToken ct);

    Task SignInAsync(Guid userId);
}