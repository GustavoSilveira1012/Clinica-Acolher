using ClinicaAcolher.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicaAcolher.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    // Cada índice único do banco corresponde a uma regra de negócio
    private static readonly Dictionary<string, (string Rule, string Message)> UniqueIndexRules = new()
    {
        ["IX_professionals_Email"] =
            ("RN-PRO-01", "Já existe um profissional cadastrado com este e-mail."),
        ["IX_therapeutic_relationships_PatientId_ProfessionalId"] =
            ("RN-VIN-02", "Este paciente já tem um vínculo em aberto com este profissional."),
    };

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg &&
            pg.ConstraintName is not null &&
            UniqueIndexRules.TryGetValue(pg.ConstraintName, out var rule))
        {
            throw new ConflictException(rule.Rule, rule.Message);
        }
    }
}