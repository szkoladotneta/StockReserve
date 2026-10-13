using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using StockReserve.Api.Data;
using StockReserve.Api.Domain;
using StockReserve.Api.Features.Products;
using StockReserve.Api.Features.Reservations;
using StockReserve.Api.Infrastructure;
using StockReserve.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Db")));

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection(ReservationOptions.SectionName));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IFraudScreening, FakeFraudScreening>();
builder.Services.AddSingleton<INotificationSink, LoggingNotificationSink>();

// Confirmation events flow from the request pipeline to the NotificationWorker through this channel.
var confirmations = Channel.CreateBounded<ReservationConfirmed>(new BoundedChannelOptions(capacity: 8)
{
    FullMode = BoundedChannelFullMode.DropWrite,
    SingleReader = true,
    SingleWriter = false,
});
builder.Services.AddSingleton(confirmations.Reader);
builder.Services.AddSingleton(confirmations.Writer);

builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<ExpirySweeper>();

builder.Services.AddHostedService<ExpiryWorker>();
builder.Services.AddHostedService<NotificationWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (app.Environment.IsDevelopment() && !db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Sku = "KB-01", Name = "Mechanical keyboard", OnHand = 5, Available = 5 },
            new Product { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Sku = "MS-02", Name = "Limited edition mouse", OnHand = 1, Available = 1 },
            new Product { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Sku = "MON-03", Name = "27\" monitor", OnHand = 10, Available = 10 });
        db.SaveChanges();
    }
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).WithTags("Health");

app.MapProductEndpoints();
app.MapReservationEndpoints();

app.Run();

// Exposes the implicit Program class to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
