using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Domain.Common;

namespace ClinicaAcolher.Api.Workers;

public sealed class SeriesGenerationWorker(
    IServiceScopeFactory scopes, TimeProvider clock, ILogger<SeriesGenerationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, clock);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            IReadOnlyList<Guid> dueIds;
            using (var scope = scopes.CreateScope())
            {
                dueIds = await scope.ServiceProvider.GetRequiredService<SeriesService>().ListDueAsync(ct);
            }

            foreach (var id in dueIds)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<SeriesService>();
                    var generated = await service.GenerateAsync(id, ct);
                    logger.LogInformation("Série {SeriesId}: {Count} sessões geradas", id, generated);
                }
                catch (Exception e) when (e is DomainException or ConflictException or NotFoundException)
                {
                    logger.LogWarning("Série {SeriesId} não gerou sessões: {Reason}", id, e.Message);
                }
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "Falha na rotina de geração de sessões das séries");
        }
    }
}