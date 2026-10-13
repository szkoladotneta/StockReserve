using System.Threading.Channels;
using StockReserve.Api.Infrastructure;

namespace StockReserve.Api.Workers;

/// <summary>Drains the confirmation channel and hands each message to the notification sink (e-mail gateway).</summary>
public sealed class NotificationWorker(
    ChannelReader<ReservationConfirmed> confirmations,
    INotificationSink sink,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification worker started");

        try
        {
            await foreach (var message in confirmations.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await sink.SendAsync(message, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send notification for reservation {ReservationId}", message.ReservationId);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Notification worker stopping");
        }
    }
}
