using StockReserve.Api.Features.Products;
using StockReserve.Tests.Infrastructure;

namespace StockReserve.Tests;

public sealed class ProductTests
{
    [Fact]
    public async Task Creating_a_product_makes_all_units_available()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();

        var created = await client.CreateProductAsync(onHand: 7, sku: "kb-42");

        Assert.Equal("KB-42", created.Sku);
        Assert.Equal(7, created.OnHand);
        Assert.Equal(7, created.Available);

        var fetched = await client.GetProductAsync(created.Id);
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Creating_a_product_with_a_duplicate_sku_returns_409()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();
        await client.CreateProductAsync(onHand: 1, sku: "DUP-1");

        var response = await client.PostAsJsonAsync("/products", new CreateProductRequest("dup-1", "Another", 1), ApiClientExtensions.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_product_returns_404()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();

        var response = await client.GetAsync($"/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
