using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MyChatBot.Api.Data;

namespace MyChatBot.Api.Endpoints;

/// <summary>
/// GET /api/db-status - proves the database is reachable and pgvector is switched on.
/// Handy while learning; also a quick "is the database okay?" check later in AWS.
/// </summary>
public class DbStatusEndpoint(AppDbContext db) : EndpointWithoutRequest<DbStatusResponse>
{
    public override void Configure()
    {
        Get("/api/db-status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Asks Postgres itself which version of the "vector" extension is installed.
        // (The column must be aliased "Value" for EF Core to read a plain string result.)
        var pgvectorVersion = await db.Database
            .SqlQuery<string>($"SELECT extversion AS \"Value\" FROM pg_extension WHERE extname = 'vector'")
            .FirstOrDefaultAsync(ct);

        // These two lines are plain LINQ: EF Core turns them into "SELECT COUNT(*) FROM ...".
        var documentCount = await db.Documents.CountAsync(ct);
        var chunkCount = await db.Chunks.CountAsync(ct);

        await Send.OkAsync(new DbStatusResponse(pgvectorVersion, documentCount, chunkCount), ct);
    }
}

public record DbStatusResponse(string? PgvectorVersion, int Documents, int Chunks);
