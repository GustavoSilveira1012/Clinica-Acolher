using ClinicaAcolher.Application.Relationships;

namespace ClinicaAcolher.Application.Common;

public sealed class AccessControl(ICurrentUser user, IRelationshipRepository relationships)
{
    public bool IsClinicAdmin => user.IsClinicAdmin;

    /// <summary>
    /// O profissional cujo escopo se aplica. Nulo para a administração da clínica, que vê tudo.
    /// Qualquer outro perfil não tem acesso.
    /// </summary>
    public Guid? ProfessionalScope =>
        user.IsClinicAdmin
            ? null
            : user.ProfessionalId ?? throw new ForbiddenException("Seu perfil não tem acesso a esta área.");

    public void EnsureClinicAdmin()
    {
        if (!user.IsClinicAdmin)
            throw new ForbiddenException("Apenas a administração da clínica pode fazer isso.");
    }

    // RN-PER-01: o profissional só acessa pacientes com quem tem ou teve vínculo
    public async Task EnsureCanAccessPatientAsync(Guid patientId, CancellationToken ct)
    {
        if (user.IsClinicAdmin)
            return;

        if (user.ProfessionalId is { } professionalId &&
            await relationships.ExistsAsync(professionalId, patientId, ct))
            return;

        // RN-SEG-05: a resposta é idêntica à de um paciente que não existe
        throw new NotFoundException("Paciente não encontrado.");
    }
}