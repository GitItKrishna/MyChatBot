using Microsoft.Extensions.Options;
using MyChatBot.Api.Pdf;

namespace MyChatBot.Api.Chunking;

/// <summary>
/// Cuts page text into overlapping chunks of at most MaxChars characters.
///
/// For each chunk it looks for the most natural place to stop, in this order:
///   1. a paragraph break, 2. the end of a sentence, 3. any whitespace, 4. a hard cut.
/// It only looks in the second half of the allowed window, so chunks never become tiny.
/// The next chunk then starts OverlapChars before the previous one ended.
/// </summary>
public class TextChunker : ITextChunker
{
    private static readonly string[] SentenceEnds = [". ", "! ", "? ", ".\n", "!\n", "?\n"];

    private readonly int _maxChars;
    private readonly int _overlapChars;

    public TextChunker(IOptions<ChunkingOptions> options)
    {
        _maxChars = options.Value.MaxChars;
        _overlapChars = options.Value.OverlapChars;

        if (_maxChars < 100)
            throw new ArgumentException("Chunking:MaxChars must be at least 100.");

        // Each chunk must move forward by more than half its length, otherwise it could loop forever.
        if (_overlapChars < 0 || _overlapChars >= _maxChars / 2)
            throw new ArgumentException("Chunking:OverlapChars must be between 0 and MaxChars / 2 (exclusive).");
    }

    public IReadOnlyList<TextChunk> Chunk(IEnumerable<PageText> pages)
    {
        var chunks = new List<TextChunk>();
        var index = 0; // runs across the whole document, matching the unique (DocumentId, ChunkIndex) index

        foreach (var page in pages)
        {
            foreach (var content in SplitPage(Normalize(page.Text)))
            {
                chunks.Add(new TextChunk(page.PageNumber, index++, content));
            }
        }

        return chunks;
    }

    /// <summary>
    /// Tidies PDF text: unifies line endings and collapses runs of spaces. Single line breaks inside
    /// a paragraph become spaces (PDFs break lines mid-sentence); blank lines stay as paragraph breaks.
    /// </summary>
    internal static string Normalize(string text)
    {
        var unified = text.Replace("\r\n", "\n").Replace('\r', '\n');

        var paragraphs = unified
            .Split("\n\n", StringSplitOptions.None)
            .Select(p => string.Join(' ', p.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)))
            .Where(p => p.Length > 0);

        return string.Join("\n\n", paragraphs);
    }

    private IEnumerable<string> SplitPage(string text)
    {
        var start = 0;

        while (start < text.Length)
        {
            var end = Math.Min(start + _maxChars, text.Length);
            if (end < text.Length)
                end = FindBreak(text, start, end);

            var chunk = text[start..end].Trim();
            if (chunk.Length > 0)
                yield return chunk;

            if (end >= text.Length)
                yield break;

            start = NextStart(text, start, end);
        }
    }

    /// <summary>Finds where to end the chunk, searching backwards from 'limit' but not before the half-way point.</summary>
    private static int FindBreak(string text, int start, int limit)
    {
        var earliest = start + (limit - start) / 2;
        var window = text.AsSpan(earliest, limit - earliest);

        var paragraph = window.LastIndexOf("\n\n");
        if (paragraph >= 0)
            return earliest + paragraph + 2;

        var sentence = -1;
        foreach (var marker in SentenceEnds)
            sentence = Math.Max(sentence, window.LastIndexOf(marker));
        if (sentence >= 0)
            return earliest + sentence + 2;

        var space = window.LastIndexOf(' ');
        if (space >= 0)
            return earliest + space + 1;

        return limit; // one enormous "word" with no spaces: cut hard
    }

    /// <summary>Where the next chunk begins: 'overlap' characters back, moved forward to the start of a word.</summary>
    private int NextStart(string text, int start, int end)
    {
        var next = Math.Max(end - _overlapChars, start + 1);

        // Do not begin in the middle of a word.
        while (next < end && !char.IsWhiteSpace(text[next - 1]))
            next++;

        return next;
    }
}
