using Microsoft.EntityFrameworkCore;

namespace MyChatBot.Api.Data;

/// <summary>
/// "Bootstraps" the database: every time the API starts, it applies any migrations that have
/// not been applied yet. On an empty database that means: enable pgvector, create the tables,
/// create the index. On an up-to-date database it does nothing, so it is safe on every start.
/// </summary>
public class DatabaseInitializer(IServiceScopeFactory scopeFactory, ILogger<DatabaseInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // A DbContext is short-lived, so we create our own "scope" instead of injecting it.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        logger.LogInformation("Applying {Count} pending database migration(s): {Names}",
            pending.Count, string.Join(", ", pending));

        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
