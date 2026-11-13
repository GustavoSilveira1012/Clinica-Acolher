namespace ClinicaAcolher.Application.Common;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
    Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct);
}
