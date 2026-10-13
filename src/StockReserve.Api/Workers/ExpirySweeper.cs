using Microsoft.EntityFrameworkCore;
using StockReserve.Api.Data;
using StockReserve.Api.Domain;

namespace StockReserve.Api.Workers;

/// <summary>
/// One unit of expiry work: releases stock held by reservations whose TTL has elapsed.
/// Kept separate from the hosted service so it can be run on demand (tests, /admin/sweep).
/// </summary>
public sealed class ExpirySweeper(AppDbContext db, TimeProvider clock, ILogger<ExpirySweeper> logger)
{
    /// <returns>The number of reservations that were expired in this sweep.</returns>
    public async Task<int> SweepAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var expired = await db.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= now)
            .ToListAsync(ct);

        foreach (var reservation in expired)
        {
            var product = await db.Products.FindAsync([reservation.ProductId], ct);
            if (product is null)
            {
                logger.LogWarning("Reservation {ReservationId} points at a missing product {ProductId}", reservation.Id, reservation.ProductId);
                continue;
            }

            product.Available += reservation.Quantity;
            reservation.Status = ReservationStatus.Expired;

            logger.LogInformation(
                "Reservation {ReservationId} expired; released {Quantity} x {ProductId}",
                reservation.Id, reservation.Quantity, reservation.ProductId);
        }

        await db.SaveChangesAsync(ct);

        return expired.Count;
    }
}
