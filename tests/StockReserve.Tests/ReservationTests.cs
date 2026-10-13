using StockReserve.Api.Domain;
using StockReserve.Tests.Infrastructure;

namespace StockReserve.Tests;

public sealed class ReservationTests
{
    [Fact]
    public async Task Reserving_stock_decrements_available_but_not_on_hand()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 5);

        var reservation = await client.ReserveOkAsync(product.Id, quantity: 2);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Equal(2, reservation.Quantity);

        var after = await client.GetProductAsync(product.Id);
        Assert.Equal(5, after.OnHand);
        Assert.Equal(3, after.Available);
    }

    [Fact]
    public async Task Reserving_more_than_available_returns_409_and_changes_nothing()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 2);

        var response = await client.ReserveAsync(product.Id, quantity: 3);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var after = await client.GetProductAsync(product.Id);
        Assert.Equal(2, after.Available);
    }

    [Fact]
    public async Task Blocked_customer_returns_403_and_changes_nothing()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 2);

        var response = await client.ReserveAsync(product.Id, quantity: 1, customerEmail: "ktos@fraud.example");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var after = await client.GetProductAsync(product.Id);
        Assert.Equal(2, after.Available);
    }

    [Fact]
    public async Task Confirming_decrements_on_hand_and_sends_a_notification()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 5);
        var reservation = await client.ReserveOkAsync(product.Id, quantity: 2, customerEmail: "bartek@example.com");

        var response = await client.ConfirmAsync(reservation.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var confirmed = await client.GetReservationAsync(reservation.Id);
        Assert.Equal(ReservationStatus.Confirmed, confirmed.Status);

        var after = await client.GetProductAsync(product.Id);
        Assert.Equal(3, after.OnHand);
        Assert.Equal(3, after.Available);

        var sent = await app.Notifications.WaitForAsync(expectedCount: 1, timeout: TimeSpan.FromSeconds(5));
        var message = Assert.Single(sent);
        Assert.Equal(reservation.Id, message.ReservationId);
        Assert.Equal("bartek@example.com", message.CustomerEmail);
    }

    [Fact]
    public async Task Cancelling_restores_available_stock()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 5);
        var reservation = await client.ReserveOkAsync(product.Id, quantity: 2);

        var response = await client.CancelAsync(reservation.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cancelled = await client.GetReservationAsync(reservation.Id);
        Assert.Equal(ReservationStatus.Cancelled, cancelled.Status);

        var after = await client.GetProductAsync(product.Id);
        Assert.Equal(5, after.Available);
        Assert.Equal(5, after.OnHand);
    }

    [Fact]
    public async Task Cancelling_twice_returns_409()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        var product = await client.CreateProductAsync(onHand: 5);
        var reservation = await client.ReserveOkAsync(product.Id, quantity: 2);
        await client.CancelAsync(reservation.Id);

        var second = await client.CancelAsync(reservation.Id);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
