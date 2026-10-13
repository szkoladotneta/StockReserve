using StockReserve.Api.Domain;

namespace StockReserve.Api.Features.Reservations;

public sealed record ReserveRequest(Guid ProductId, int Quantity, string CustomerEmail);

public sealed record ReservationResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    string CustomerEmail,
    ReservationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt)
{
    public static ReservationResponse From(Reservation reservation) => new(
        reservation.Id,
        reservation.ProductId,
        reservation.Quantity,
        reservation.CustomerEmail,
        reservation.Status,
        reservation.CreatedAt,
        reservation.ExpiresAt);
}

public enum ReserveStatus
{
    Created,
    InvalidRequest,
    ProductNotFound,
    InsufficientStock,
    Blocked,
}

public sealed record ReserveOutcome(ReserveStatus Status, Reservation? Reservation = null, string? Detail = null);

public enum ConfirmStatus
{
    Confirmed,
    NotFound,
    NotActive,
}

public enum CancelStatus
{
    Cancelled,
    NotFound,
    NotActive,
}
