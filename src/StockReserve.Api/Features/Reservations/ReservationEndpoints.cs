using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StockReserve.Api.Data;
using StockReserve.Api.Workers;

namespace StockReserve.Api.Features.Reservations;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservations").WithTags("Reservations");

        group.MapPost("/", ReserveAsync);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetReservation");
        group.MapPost("/{id:guid}/confirm", ConfirmAsync);
        group.MapPost("/{id:guid}/cancel", CancelAsync);

        // Operational endpoint: runs one expiry sweep immediately instead of waiting for the worker tick.
        app.MapPost("/admin/sweep", async (ExpirySweeper sweeper, CancellationToken ct) =>
        {
            var expired = await sweeper.SweepAsync(ct);
            return TypedResults.Ok(new { expired });
        }).WithTags("Admin");

        return app;
    }

    private static async Task<Results<Created<ReservationResponse>, NotFound<ProblemDetails>, Conflict<ProblemDetails>, ProblemHttpResult, ValidationProblem>> ReserveAsync(
        ReserveRequest request,
        ReservationService service,
        CancellationToken ct)
    {
        var outcome = await service.ReserveAsync(request, ct);

        if (outcome.Status == ReserveStatus.Created)
        {
            var reservation = outcome.Reservation!;
            return TypedResults.Created($"/reservations/{reservation.Id}", ReservationResponse.From(reservation));
        }

        if (outcome.Status == ReserveStatus.ProductNotFound)
        {
            return TypedResults.NotFound(new ProblemDetails { Title = "Product not found", Status = StatusCodes.Status404NotFound });
        }

        if (outcome.Status == ReserveStatus.InsufficientStock)
        {
            return TypedResults.Conflict(new ProblemDetails { Title = "Insufficient stock", Detail = outcome.Detail, Status = StatusCodes.Status409Conflict });
        }

        if (outcome.Status == ReserveStatus.Blocked)
        {
            return TypedResults.Problem(title: "Reservation blocked", detail: outcome.Detail, statusCode: StatusCodes.Status403Forbidden);
        }

        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = [outcome.Detail ?? "invalid request"],
        });
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound>> GetByIdAsync(Guid id, AppDbContext db, CancellationToken ct)
    {
        var reservation = await db.Reservations.FindAsync([id], ct);
        return reservation is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ReservationResponse.From(reservation));
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, Conflict<ProblemDetails>>> ConfirmAsync(
        Guid id,
        ReservationService service,
        AppDbContext db,
        CancellationToken ct)
    {
        var status = await service.ConfirmAsync(id, ct);

        if (status == ConfirmStatus.NotFound)
        {
            return TypedResults.NotFound();
        }

        if (status == ConfirmStatus.NotActive)
        {
            return TypedResults.Conflict(new ProblemDetails { Title = "Reservation is not active", Status = StatusCodes.Status409Conflict });
        }

        var reservation = await db.Reservations.FindAsync([id], ct);
        return TypedResults.Ok(ReservationResponse.From(reservation!));
    }

    private static async Task<Results<Ok<ReservationResponse>, NotFound, Conflict<ProblemDetails>>> CancelAsync(
        Guid id,
        ReservationService service,
        AppDbContext db,
        CancellationToken ct)
    {
        var status = await service.CancelAsync(id, ct);

        if (status == CancelStatus.NotFound)
        {
            return TypedResults.NotFound();
        }

        if (status == CancelStatus.NotActive)
        {
            return TypedResults.Conflict(new ProblemDetails { Title = "Reservation is not active", Status = StatusCodes.Status409Conflict });
        }

        var reservation = await db.Reservations.FindAsync([id], ct);
        return TypedResults.Ok(ReservationResponse.From(reservation!));
    }
}
