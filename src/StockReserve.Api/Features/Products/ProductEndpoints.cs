using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StockReserve.Api.Data;
using StockReserve.Api.Domain;

namespace StockReserve.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetProduct");
        group.MapPost("/", CreateAsync);

        return app;
    }

    private static async Task<Ok<PagedResponse<ProductResponse>>> ListAsync(
        AppDbContext db,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20)
    {
        var query = db.Products.AsNoTracking().OrderBy(p => p.Sku);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse(p.Id, p.Sku, p.Name, p.OnHand, p.Available))
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResponse<ProductResponse>(items, page, pageSize, totalCount));
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound>> GetByIdAsync(Guid id, AppDbContext db, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(product));
    }

    private static async Task<Results<Created<ProductResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductRequest request,
        AppDbContext db,
        CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Sku)) errors["sku"] = ["sku is required"];
        if (string.IsNullOrWhiteSpace(request.Name)) errors["name"] = ["name is required"];
        if (request.OnHand < 0) errors["onHand"] = ["onHand cannot be negative"];
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var sku = request.Sku.Trim().ToUpperInvariant();
        if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
        {
            return TypedResults.Conflict($"Product with SKU '{sku}' already exists");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = request.Name.Trim(),
            OnHand = request.OnHand,
            Available = request.OnHand,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/products/{product.Id}", ToResponse(product));
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Sku, product.Name, product.OnHand, product.Available);
}
