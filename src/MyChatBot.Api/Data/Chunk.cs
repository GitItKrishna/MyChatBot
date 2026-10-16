using Pgvector;

namespace MyChatBot.Api.Data;

/// <summary>
/// One row per piece of text cut from a PDF. This is the table the chatbot actually searches.
/// </summary>
public class Chunk
{
    public int Id { get; set; }

    /// <summary>Which PDF this chunk came from (a "foreign key" pointing at documents.Id).</summary>
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    /// <summary>PDF page the text came from. This is what lets us cite "page 7".</summary>
    public int PageNumber { get; set; }

    /// <summary>Order of this chunk inside the document (0, 1, 2, ...).</summary>
    public int ChunkIndex { get; set; }

    /// <summary>The actual text.</summary>
    public required string Content { get; set; }

    /// <summary>
    /// The "meaning" of Content as 1024 numbers (made by Amazon Titan in step 05).
    /// Similar meaning = nearby vectors. Pgvector.Vector is the .NET type for the vector column.
    /// </summary>
    public required Vector Embedding { get; set; }
}
