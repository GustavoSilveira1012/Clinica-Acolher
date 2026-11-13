using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;

namespace ClinicaAcolher.Domain.Relationships;

public sealed class TherapeuticRelationship : Entity
{
    public Guid PatientId {get; private set;}
    public Guid ProfessionalId {get; private set;}
    public RelationshipStatus Status {get; private set;}
    public DateTimeOffset StartedAt {get; private set;}
    public DateTimeOffset? EndedAt {get; private set;}
    public string? EndReason {get; private set;}

    //RN-VIN-04: só vínculo ativo permite agendar novas sessões
    public bool AllowsScheduling => Status == RelationshipStatus.Active;

    private TherapeuticRelationship() {}

    public static TherapeuticRelationship Start (Patient patient, Professional professional, DateTimeOffset now)
    {
        if (!patient.Active)
            throw new DomainException("RN-VIN-01", "Não é possível iniciar vínculo com paciente desativado.");

        if (!professional.CanAttend)
            throw new DomainException("RN-VIN-01", "O profissional precisa estar aprovado para iniciar um vínculo.");

        //Enquanto o cadastro de responsável legal não existe, menores ficam bloqueados por segurança.
        if (patient.IsMinor(DateOnly.FromDateTime(now.UtcDateTime)))
            throw new DomainException("RN-PAC-03", "Pacientes menores de idade precisam de responsável legal cadastrado.");

        return new TherapeuticRelationship
        {
            PatientId = patient.Id,
            ProfessionalId = professional.Id,
            Status = RelationshipStatus.Active,
            StartedAt = now,
        };
    }

    public void Suspend()
    {
        EnsureStatus("Só um vínculo ativo pode ser suspenso.", RelationshipStatus.Active);
        Status = RelationshipStatus.Suspended;
    }

    public void Resume()
    {
        EnsureStatus("Só um vínculo suspenso pode ser retomado.", RelationshipStatus.Suspended);
        Status = RelationshipStatus.Active;
    }

    public void End(string reason, DateTimeOffset now)
    {
        EnsureStatus("Este vínculo já foi encerrado.", RelationshipStatus.Active, RelationshipStatus.Suspended);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("RN-VIN-04", "Informe o motivo do encerramento.");

        Status = RelationshipStatus.Ended;
        EndedAt = now;
        EndReason = reason.Trim();
    }

    private void EnsureStatus(string message, params RelationshipStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new DomainException("RN-VIN-03", message);
    }
}
