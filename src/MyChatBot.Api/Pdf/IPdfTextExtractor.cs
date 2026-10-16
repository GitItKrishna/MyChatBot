namespace MyChatBot.Api.Pdf;

/// <summary>
/// Reads a PDF and returns its text one page at a time. Keeping the page boundary is what
/// lets answers cite "page 7" later. Hidden behind an interface so the PDF library can be
/// swapped (or faked in tests) without touching the rest of the app.
/// </summary>
public interface IPdfTextExtractor
{
    /// <exception cref="InvalidPdfException">The stream is not a readable PDF.</exception>
    IReadOnlyList<PageText> ExtractPages(Stream pdf);

    /// <summary>Counts pages without extracting any text, so oversized files are rejected cheaply.</summary>
    /// <exception cref="InvalidPdfException">The stream is not a readable PDF.</exception>
    int CountPages(Stream pdf);
}

public class InvalidPdfException(string message, Exception? inner = null) : Exception(message, inner);
