using StockReserve.Tests.Infrastructure;

namespace StockReserve.Tests;

public sealed class HealthTests
{
    [Fact]
    public async Task Health_endpoint_responds_ok()
    {
        await using var app = new ApiFactory();
        var client = app.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
