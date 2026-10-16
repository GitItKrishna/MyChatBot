namespace MyChatBot.Api.Chunking;

/// <summary>Chunking settings, read from the "Chunking" section of appsettings.json.</summary>
public class ChunkingOptions
{
    public const string SectionName = "Chunking";

    /// <summary>Upper limit for the length of one chunk, in characters.</summary>
    public int MaxChars { get; set; } = 800;

    /// <summary>
    /// How much of the end of one chunk is repeated at the start of the next. The repeat means
    /// a sentence that straddles a boundary still appears whole in at least one chunk.
    /// </summary>
    public int OverlapChars { get; set; } = 150;
}
