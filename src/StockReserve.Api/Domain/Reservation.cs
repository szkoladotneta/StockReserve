namespace StockReserve.Api.Domain;

public enum ReservationStatus
{
    Active = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3,
}

public sealed class Reservation
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public required string CustomerEmail { get; set; }

    public ReservationStatus Status { get; set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp after which the held stock is released by the expiry worker.</summary>
    public DateTime ExpiresAt { get; set; }
}
