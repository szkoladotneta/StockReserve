using Microsoft.Extensions.Options;
using StockReserve.Api.Features.Reservations;

namespace StockReserve.Api.Workers;

/// <summary>Periodically runs <see cref="ExpirySweeper"/> so that abandoned reservations give their stock back.</summary>
public sealed class ExpiryWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ReservationOptions> options,
    TimeProvider clock,
    ILogger<ExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var period = TimeSpan.FromSeconds(options.Value.SweepIntervalSeconds);
        logger.LogInformation("Expiry worker started; sweeping every {Period}", period);

        using var timer = new PeriodicTimer(period, clock);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var sweeper = scope.ServiceProvider.GetRequiredService<ExpirySweeper>();

                    var expired = await sweeper.SweepAsync(stoppingToken);
                    if (expired > 0)
                    {
                        logger.LogInformation("Expiry sweep released {Count} reservation(s)", expired);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Expiry sweep failed; will retry on the next tick");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Expiry worker stopping");
        }
    }
}
