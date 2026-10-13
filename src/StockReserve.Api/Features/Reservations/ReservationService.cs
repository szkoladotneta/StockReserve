using System.Threading.Channels;
using Microsoft.Extensions.Options;
using StockReserve.Api.Data;
using StockReserve.Api.Domain;
using StockReserve.Api.Infrastructure;

namespace StockReserve.Api.Features.Reservations;

public sealed class ReservationService(
    AppDbContext db,
    IFraudScreening fraudScreening,
    TimeProvider clock,
    IOptions<ReservationOptions> options,
    ChannelWriter<ReservationConfirmed> confirmations,
    ILogger<ReservationService> logger)
{
    public async Task<ReserveOutcome> ReserveAsync(ReserveRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            return new ReserveOutcome(ReserveStatus.InvalidRequest, Detail: "quantity must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            return new ReserveOutcome(ReserveStatus.InvalidRequest, Detail: "customerEmail is required");
        }

        var product = await db.Products.FindAsync([request.ProductId], ct);
        if (product is null)
        {
            return new ReserveOutcome(ReserveStatus.ProductNotFound);
        }

        if (product.Available < request.Quantity)
        {
            return new ReserveOutcome(
                ReserveStatus.InsufficientStock,
                Detail: $"requested {request.Quantity}, only {product.Available} available");
        }

        var verdict = await fraudScreening.ScreenAsync(request.CustomerEmail, ct);
        if (!verdict.Allowed)
        {
            return new ReserveOutcome(ReserveStatus.Blocked, Detail: verdict.Reason);
        }

        product.Available -= request.Quantity;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = request.Quantity,
            CustomerEmail = request.CustomerEmail.Trim(),
            Status = ReservationStatus.Active,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
            ExpiresAt = DateTime.Now.AddMinutes(options.Value.TtlMinutes),
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Reserved {Quantity} x {ProductId} for {CustomerEmail}; reservation {ReservationId} expires at {ExpiresAt}",
            reservation.Quantity, reservation.ProductId, reservation.CustomerEmail, reservation.Id, reservation.ExpiresAt);

        return new ReserveOutcome(ReserveStatus.Created, reservation);
    }

    public async Task<ConfirmStatus> ConfirmAsync(Guid reservationId, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([reservationId], ct);
        if (reservation is null)
        {
            return ConfirmStatus.NotFound;
        }

        var product = await db.Products.FindAsync([reservation.ProductId], ct)
            ?? throw new InvalidOperationException($"Product {reservation.ProductId} is missing for reservation {reservationId}");

        reservation.Status = ReservationStatus.Confirmed;
        product.OnHand -= reservation.Quantity;

        await db.SaveChangesAsync(ct);

        var message = new ReservationConfirmed(reservation.Id, reservation.ProductId, reservation.Quantity, reservation.CustomerEmail);
        if (!confirmations.TryWrite(message))
        {
            logger.LogWarning("Notification channel is full; confirmation for {ReservationId} was not queued", reservation.Id);
        }

        return ConfirmStatus.Confirmed;
    }

    public async Task<CancelStatus> CancelAsync(Guid reservationId, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([reservationId], ct);
        if (reservation is null)
        {
            return CancelStatus.NotFound;
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            return CancelStatus.NotActive;
        }

        var product = await db.Products.FindAsync([reservation.ProductId], ct)
            ?? throw new InvalidOperationException($"Product {reservation.ProductId} is missing for reservation {reservationId}");

        reservation.Status = ReservationStatus.Cancelled;
        product.Available += reservation.Quantity;

        await db.SaveChangesAsync(ct);

        return CancelStatus.Cancelled;
    }
}
