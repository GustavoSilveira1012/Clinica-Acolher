namespace ClinicaAcolher.Application.Common;

/// <summary>
/// O que aconteceu, sem nenhum conteúdo clínico: só ações e identificadores (RN-PRT-09).
/// </summary>
public sealed record AuditEvent(string Action, string ResourceType, Guid ResourceId, Guid? PatientId);

/// <summary>
/// Trilha de auditoria (RN-PRT-05, RN-SEG-08). O registro é gravado na mesma transação
/// do próximo IUnitOfWork.SaveChangesAsync: os dois acontecem juntos, ou nenhum acontece.
/// </summary>
public interface IAuditTrail
{
    void Record(AuditEvent auditEvent);
}