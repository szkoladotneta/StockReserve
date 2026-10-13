using System.Text.Json;
using System.Text.Json.Serialization;
using StockReserve.Api.Features.Products;
using StockReserve.Api.Features.Reservations;

namespace StockReserve.Tests.Infrastructure;

/// <summary>Small, readable wrappers around the HTTP API so tests read like scenarios.</summary>
public static class ApiClientExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<ProductResponse> CreateProductAsync(this HttpClient client, int onHand, string? sku = null)
    {
        sku ??= "SKU-" + Guid.NewGuid().ToString("N")[..8];

        var response = await client.PostAsJsonAsync("/products", new CreateProductRequest(sku, $"Product {sku}", onHand), Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<ProductResponse>(Json))!;
    }

    public static async Task<ProductResponse> GetProductAsync(this HttpClient client, Guid productId)
    {
        var response = await client.GetAsync($"/products/{productId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<ProductResponse>(Json))!;
    }

    public static Task<HttpResponseMessage> ReserveAsync(this HttpClient client, Guid productId, int quantity, string customerEmail = "ania@example.com") =>
        client.PostAsJsonAsync("/reservations", new ReserveRequest(productId, quantity, customerEmail), Json);

    public static async Task<ReservationResponse> ReserveOkAsync(this HttpClient client, Guid productId, int quantity, string customerEmail = "ania@example.com")
    {
        var response = await client.ReserveAsync(productId, quantity, customerEmail);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<ReservationResponse>(Json))!;
    }

    public static async Task<ReservationResponse> GetReservationAsync(this HttpClient client, Guid reservationId)
    {
        var response = await client.GetAsync($"/reservations/{reservationId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.Content.ReadFromJsonAsync<ReservationResponse>(Json))!;
    }

    public static Task<HttpResponseMessage> ConfirmAsync(this HttpClient client, Guid reservationId) =>
        client.PostAsync($"/reservations/{reservationId}/confirm", content: null);

    public static Task<HttpResponseMessage> CancelAsync(this HttpClient client, Guid reservationId) =>
        client.PostAsync($"/reservations/{reservationId}/cancel", content: null);
}
