using System.Collections.Concurrent;
using StockReserve.Api.Infrastructure;

namespace StockReserve.Tests.Infrastructure;

/// <summary>
/// Replaces the e-mail gateway in tests: remembers every message instead of sending it.
/// Keeps the same simulated latency as the real sink so the worker behaves the same way.
/// </summary>
public sealed class RecordingNotificationSink : INotificationSink
{
    private readonly ConcurrentQueue<ReservationConfirmed> _sent = new();

    public TimeSpan SimulatedLatency { get; set; } = TimeSpan.FromMilliseconds(20);

    public IReadOnlyCollection<ReservationConfirmed> Sent => _sent.ToArray();

    public async Task SendAsync(ReservationConfirmed message, CancellationToken ct)
    {
        await Task.Delay(SimulatedLatency, ct);
        _sent.Enqueue(message);
    }

    /// <summary>Waits until at least <paramref name="expectedCount"/> messages were sent, or the timeout elapses.</summary>
    public async Task<IReadOnlyCollection<ReservationConfirmed>> WaitForAsync(int expectedCount, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (_sent.Count < expectedCount && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        return _sent.ToArray();
    }
}
