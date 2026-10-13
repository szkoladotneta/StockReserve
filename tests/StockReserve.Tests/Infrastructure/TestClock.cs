namespace StockReserve.Tests.Infrastructure;

/// <summary>
/// A <see cref="TimeProvider"/> whose "now" only moves when the test says so.
/// Anything in the application that asks the injected TimeProvider for the time sees this value.
/// (For production-grade needs see Microsoft.Extensions.TimeProvider.Testing's FakeTimeProvider.)
/// </summary>
public sealed class TestClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;

    public void SetUtcNow(DateTimeOffset value) => _now = value;
}
