using MyChatBot.Api.Pdf;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace MyChatBot.Api.Tests;

public class PdfPigTextExtractorTests
{
    /// <summary>Builds a small PDF in memory: one entry in 'pageTexts' = one page. No files needed.</summary>
    private static MemoryStream BuildPdf(params string[] pageTexts)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        foreach (var text in pageTexts)
        {
            var page = builder.AddPage(595, 842); // A4 in points
            if (text.Length > 0)
                page.AddText(text, 12, new PdfPoint(50, 780), font);
        }

        return new MemoryStream(builder.Build());
    }

    [Fact]
    public void Returns_one_entry_per_page_with_one_based_numbers()
    {
        using var pdf = BuildPdf("First page text", "Second page text", "Third page text");

        var pages = new PdfPigTextExtractor().ExtractPages(pdf);

        Assert.Equal([1, 2, 3], pages.Select(p => p.PageNumber));
        Assert.Contains("First page text", pages[0].Text);
        Assert.Contains("Second page text", pages[1].Text);
        Assert.Contains("Third page text", pages[2].Text);
    }

    [Fact]
    public void A_page_without_text_is_returned_empty_not_skipped()
    {
        using var pdf = BuildPdf("Has text", "");

        var pages = new PdfPigTextExtractor().ExtractPages(pdf);

        Assert.Equal(2, pages.Count);
        Assert.True(string.IsNullOrWhiteSpace(pages[1].Text));
    }

    [Fact]
    public void Non_pdf_input_throws_InvalidPdfException()
    {
        using var notAPdf = new MemoryStream("this is not a pdf"u8.ToArray());

        Assert.Throws<InvalidPdfException>(() => new PdfPigTextExtractor().ExtractPages(notAPdf));
    }

    [Fact]
    public void CountPages_returns_the_page_count()
    {
        using var pdf = BuildPdf("one", "two", "three", "four");

        Assert.Equal(4, new PdfPigTextExtractor().CountPages(pdf));
    }
}
