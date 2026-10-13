namespace StockReserve.Api.Features.Products;

public sealed record CreateProductRequest(string Sku, string Name, int OnHand);

public sealed record ProductResponse(Guid Id, string Sku, string Name, int OnHand, int Available);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
