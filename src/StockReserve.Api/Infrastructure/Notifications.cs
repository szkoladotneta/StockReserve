namespace StockReserve.Api.Infrastructure;

/// <summary>Message published after a reservation has been confirmed (e.g. to send an e-mail).</summary>
public sealed record ReservationConfirmed(Guid ReservationId, Guid ProductId, int Quantity, string CustomerEmail);

public interface INotificationSink
{
    Task SendAsync(ReservationConfirmed message, CancellationToken ct);
}

/// <summary>Stand-in for an e-mail gateway: logs the message and simulates gateway latency.</summary>
public sealed class LoggingNotificationSink(ILogger<LoggingNotificationSink> logger) : INotificationSink
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(20);

    public async Task SendAsync(ReservationConfirmed message, CancellationToken ct)
    {
        await Task.Delay(SimulatedLatency, ct);

        logger.LogInformation(
            "Confirmation e-mail sent to {CustomerEmail} for reservation {ReservationId} ({Quantity} x {ProductId})",
            message.CustomerEmail, message.ReservationId, message.Quantity, message.ProductId);
    }
}
