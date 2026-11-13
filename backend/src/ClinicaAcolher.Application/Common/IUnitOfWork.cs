namespace ClinicaAcolher.Application.Common;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}