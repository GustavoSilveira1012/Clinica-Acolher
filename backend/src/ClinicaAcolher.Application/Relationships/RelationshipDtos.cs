using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.Application.Relationships;

public sealed record StartRelationshipRequest(Guid PatientId, Guid ProfessionalId);

public sealed record EndRelationshipRequest(string Reason);

public sealed record ActiveRelationshipResponse(Guid RelationshipId, Guid PatientId, string PatientDisplayName);

public sealed record RelationshipResponse(
    Guid Id,
    Guid PatientId,
    Guid ProfessionalId,
    RelationshipStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string? EndReason,
    bool AllowsScheduling
)
{
    public static RelationshipResponse From(TherapeuticRelationship r) => new(
        r.Id, r.PatientId, r.ProfessionalId, r.Status,
        r.StartedAt, r.EndedAt, r.EndReason, r.AllowsScheduling
    );
}
