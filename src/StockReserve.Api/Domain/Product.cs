namespace StockReserve.Api.Domain;

public sealed class Product
{
    public Guid Id { get; set; }

    public required string Sku { get; set; }

    public required string Name { get; set; }

    /// <summary>Physical units sitting in the warehouse.</summary>
    public int OnHand { get; set; }

    /// <summary>
    /// Units that are not held by an active reservation.
    /// Invariant: 0 &lt;= Available &lt;= OnHand.
    /// </summary>
    public int Available { get; set; }
}
