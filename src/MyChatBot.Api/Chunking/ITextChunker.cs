using MyChatBot.Api.Pdf;

namespace MyChatBot.Api.Chunking;

public interface ITextChunker
{
    /// <summary>Splits pages into chunks. A chunk never spans two pages, so its page number is exact.</summary>
    IReadOnlyList<TextChunk> Chunk(IEnumerable<PageText> pages);
}
