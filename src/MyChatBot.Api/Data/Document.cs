namespace MyChatBot.Api.Data;

/// <summary>
/// One row per PDF we have ingested. EF Core turns this class into the "documents" table:
/// each property becomes a column, each instance becomes a row.
/// </summary>
public class Document
{
    public int Id { get; set; }

    /// <summary>Display name, e.g. "policy.pdf". Shown later in citations.</summary>
    public required string FileName { get; set; }

    /// <summary>Where the PDF lives in S3 (used from step 06 onwards).</summary>
    public string? S3Key { get; set; }

    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>All the text pieces cut from this PDF. EF Core uses this to link the two tables.</summary>
    public List<Chunk> Chunks { get; set; } = [];
}
