using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MyChatBot.Api.Pdf;

/// <summary>
/// Extracts text with PdfPig, a pure .NET PDF reader (no native dependencies, so it also runs
/// unchanged in the Linux container we deploy to Fargate).
/// Limitation: PdfPig reads text that is stored as text. A scanned PDF (a photo of a page)
/// has no text, so those pages come back empty; that would need OCR.
/// </summary>
public class PdfPigTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PageText> ExtractPages(Stream pdf)
    {
        try
        {
            using var document = PdfDocument.Open(pdf);

            var pages = new List<PageText>(document.NumberOfPages);
            foreach (var page in document.GetPages())
            {
                // ContentOrderTextExtractor orders words the way a person reads the page
                // (instead of the raw order they happen to be stored in the file).
                pages.Add(new PageText(page.Number, ContentOrderTextExtractor.GetText(page)));
            }

            return pages;
        }
        catch (Exception ex) when (ex is not InvalidPdfException)
        {
            throw new InvalidPdfException("The file could not be read as a PDF.", ex);
        }
    }

    public int CountPages(Stream pdf)
    {
        try
        {
            using var document = PdfDocument.Open(pdf);
            return document.NumberOfPages;
        }
        catch (Exception ex)
        {
            throw new InvalidPdfException("The file could not be read as a PDF.", ex);
        }
    }
}
