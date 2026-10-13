namespace StockReserve.Api.Features.Reservations;

public sealed class ReservationOptions
{
    public const string SectionName = "Reservations";

    /// <summary>How long a reservation holds stock before the expiry worker releases it.</summary>
    public int TtlMinutes { get; set; } = 15;

    /// <summary>How often the expiry worker looks for reservations to release.</summary>
    public int SweepIntervalSeconds { get; set; } = 10;
}
