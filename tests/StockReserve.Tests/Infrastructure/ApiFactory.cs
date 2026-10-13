using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StockReserve.Api.Data;
using StockReserve.Api.Infrastructure;
using StockReserve.Api.Workers;

namespace StockReserve.Tests.Infrastructure;

/// <summary>
/// Boots the real application in-process with three test seams:
///  - a fresh SQLite file per factory (real database, real locking, no shared state between tests),
///  - a <see cref="TestClock"/> instead of the system clock,
///  - a <see cref="RecordingNotificationSink"/> instead of the e-mail gateway.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"stockreserve-tests-{Guid.NewGuid():N}.db");

    /// <summary>Fixed, well in the past, so no test depends on the wall clock of the machine running it.</summary>
    public TestClock Clock { get; } = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    public RecordingNotificationSink Notifications { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Db", $"Data Source={_dbPath};Pooling=False");
        builder.UseSetting("Reservations:TtlMinutes", "15");
        builder.UseSetting("Reservations:SweepIntervalSeconds", "3600"); // tests trigger sweeps explicitly

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);

            services.RemoveAll<INotificationSink>();
            services.AddSingleton<INotificationSink>(Notifications);
        });
    }

    /// <summary>Runs one expiry sweep, exactly like the worker tick or POST /admin/sweep would.</summary>
    public async Task<int> SweepAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var sweeper = scope.ServiceProvider.GetRequiredService<ExpirySweeper>();
        return await sweeper.SweepAsync(CancellationToken.None);
    }

    /// <summary>Looks straight into the database, bypassing the API.</summary>
    public async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        TryDeleteDatabase();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            TryDeleteDatabase();
        }
    }

    private void TryDeleteDatabase()
    {
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch (IOException)
        {
            // best effort; the file lives in the temp folder anyway
        }
    }
}
