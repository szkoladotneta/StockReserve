namespace StockReserve.Api.Infrastructure;

public readonly record struct FraudVerdict(bool Allowed, string? Reason = null);

public interface IFraudScreening
{
    Task<FraudVerdict> ScreenAsync(string customerEmail, CancellationToken ct);
}

/// <summary>
/// Stand-in for an external anti-fraud HTTP API. The latency is simulated so that the
/// application behaves like it would with a real network round-trip.
/// </summary>
public sealed class FakeFraudScreening : IFraudScreening
{
    private static readonly TimeSpan SimulatedLatency = TimeSpan.FromMilliseconds(50);

    public async Task<FraudVerdict> ScreenAsync(string customerEmail, CancellationToken ct)
    {
        await Task.Delay(SimulatedLatency, ct);

        return customerEmail.EndsWith("@fraud.example", StringComparison.OrdinalIgnoreCase)
            ? new FraudVerdict(Allowed: false, Reason: "customer is on the block list")
            : new FraudVerdict(Allowed: true);
    }
}
