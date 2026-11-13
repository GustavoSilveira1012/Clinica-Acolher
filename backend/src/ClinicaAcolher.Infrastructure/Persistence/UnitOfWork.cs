using ClinicaAcolher.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicaAcolher.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    // Cada restrição de unicidade ou exclusão mapeada corresponde a uma regra de negócio
    private static readonly Dictionary<string, (string Rule, string Message)> UniqueIndexRules = new()
    {
        ["IX_professionals_Email"] =
            ("RN-PRO-01", "Já existe um profissional cadastrado com este e-mail."),
        ["IX_therapeutic_relationships_PatientId_ProfessionalId"] =
            ("RN-VIN-02", "Este paciente já tem um vínculo em aberto com este profissional."),
        ["ex_appointments_professional_overlap"] =
            ("RN-AGD-03", "O profissional já tem uma sessão neste horário."),
        ["ex_appointments_patient_overlap"] =
            ("RN-AGD-04", "O paciente já tem uma sessão neste horário."),
    };

    public async Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await work();
        await transaction.CommitAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ExclusionViolation
            } pg &&
            pg.ConstraintName is not null &&
            UniqueIndexRules.TryGetValue(pg.ConstraintName, out var rule))
        {
            throw new ConflictException(rule.Rule, rule.Message);
        }
    }
}
