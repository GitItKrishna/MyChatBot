using Microsoft.Extensions.Options;
using MyChatBot.Api.Chunking;
using MyChatBot.Api.Pdf;

namespace MyChatBot.Api.Tests;

public class TextChunkerTests
{
    private static TextChunker CreateChunker(int maxChars = 200, int overlapChars = 40) =>
        new(Options.Create(new ChunkingOptions { MaxChars = maxChars, OverlapChars = overlapChars }));

    // A realistic-looking paragraph made of numbered sentences, so we can tell chunks apart.
    private static string Sentences(int count) =>
        string.Join(' ', Enumerable.Range(1, count).Select(i => $"This is sentence number {i}."));

    [Fact]
    public void Short_page_becomes_one_chunk()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, "A short page.")]);

        var chunk = Assert.Single(chunks);
        Assert.Equal(1, chunk.PageNumber);
        Assert.Equal(0, chunk.ChunkIndex);
        Assert.Equal("A short page.", chunk.Content);
    }

    [Fact]
    public void Long_page_is_split_and_no_chunk_exceeds_the_limit()
    {
        var chunks = CreateChunker(maxChars: 200, overlapChars: 40).Chunk([new PageText(1, Sentences(40))]);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 200, $"chunk {c.ChunkIndex} has {c.Content.Length} chars"));
    }

    [Fact]
    public void Chunks_end_at_sentence_boundaries_when_possible()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, Sentences(40))]);

        // Every chunk except the last should end on a full stop rather than mid-sentence.
        Assert.All(chunks.Take(chunks.Count - 1), c => Assert.EndsWith(".", c.Content));
    }

    [Fact]
    public void Consecutive_chunks_overlap()
    {
        var chunks = CreateChunker(maxChars: 200, overlapChars: 40).Chunk([new PageText(1, Sentences(40))]);

        for (var i = 1; i < chunks.Count; i++)
        {
            // The start of a chunk repeats the end of the previous one: its first word must appear there.
            var firstWord = chunks[i].Content.Split(' ')[0];
            Assert.Contains(firstWord, chunks[i - 1].Content);
        }
    }

    [Fact]
    public void Chunks_never_span_pages_and_keep_their_page_number()
    {
        var pages = new[]
        {
            new PageText(1, Sentences(20)),
            new PageText(2, "Second page text."),
            new PageText(3, Sentences(20)),
        };

        var chunks = CreateChunker().Chunk(pages);

        Assert.Contains(chunks, c => c is { PageNumber: 2, Content: "Second page text." });
        Assert.Equal([1, 2, 3], chunks.Select(c => c.PageNumber).Distinct());
        Assert.Equal(chunks.OrderBy(c => c.PageNumber).Select(c => c.PageNumber), chunks.Select(c => c.PageNumber));
    }

    [Fact]
    public void Chunk_index_runs_across_the_whole_document()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, Sentences(20)), new PageText(2, Sentences(20))]);

        Assert.Equal(Enumerable.Range(0, chunks.Count), chunks.Select(c => c.ChunkIndex));
    }

    [Fact]
    public void Pages_without_text_produce_no_chunks()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, ""), new PageText(2, "   \n  "), new PageText(3, "Real text.")]);

        var chunk = Assert.Single(chunks);
        Assert.Equal(3, chunk.PageNumber);
    }

    [Fact]
    public void Paragraph_breaks_are_preferred_split_points()
    {
        var first = string.Join(' ', Enumerable.Repeat("alpha", 25)); // 149 chars
        var second = string.Join(' ', Enumerable.Repeat("beta", 25));
        var chunks = CreateChunker(maxChars: 200, overlapChars: 20).Chunk([new PageText(1, $"{first}\n\n{second}")]);

        // The first chunk stops exactly at the paragraph break...
        Assert.Equal(first, chunks[0].Content);

        // ...and the second carries the overlap (the tail of the first paragraph) followed by the new one.
        Assert.StartsWith("alpha", chunks[1].Content);
        Assert.EndsWith(second, chunks[1].Content);
    }

    [Fact]
    public void Line_breaks_inside_a_paragraph_are_treated_as_spaces()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, "A sentence broken\nacross two   lines.")]);

        Assert.Equal("A sentence broken across two lines.", Assert.Single(chunks).Content);
    }

    [Fact]
    public void A_single_enormous_word_is_cut_hard_and_still_terminates()
    {
        var chunks = CreateChunker().Chunk([new PageText(1, new string('x', 1000))]);

        Assert.True(chunks.Count >= 5);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 200));
    }

    [Theory]
    [InlineData(50, 10)]    // MaxChars too small
    [InlineData(200, 100)]  // overlap not below half of MaxChars
    [InlineData(200, -1)]   // negative overlap
    public void Invalid_options_are_rejected(int maxChars, int overlapChars)
    {
        Assert.Throws<ArgumentException>(() => CreateChunker(maxChars, overlapChars));
    }
}
