using Microsoft.Extensions.Options;

namespace MyChatBot.Api.Pdf;

/// <summary>
/// Decides whether an uploaded PDF is allowed in. Returns null when it is fine, or the message
/// to show the user when it is not. Kept separate from the endpoint so /ingest can reuse it
/// and so it can be unit tested without any web plumbing.
/// </summary>
public class PdfUploadValidator(IOptions<PdfLimitsOptions> options, IPdfTextExtractor extractor)
{
    private readonly PdfLimitsOptions _limits = options.Value;

    /// <summary>The message shown when a file breaks a limit.</summary>
    public string LimitMessage =>
        $"File must be no larger than {_limits.MaxFileBytes / (1024.0 * 1024.0):0.##} MB and have no more than {_limits.MaxPages} pages.";

    /// <param name="fileLength">Size of the upload in bytes (cheap to check, so it goes first).</param>
    /// <param name="openPdf">Opens a fresh stream over the PDF; called only if the size is acceptable.</param>
    public string? Validate(long fileLength, Func<Stream> openPdf)
    {
        if (fileLength <= 0)
            return "Upload a PDF in the 'file' form field.";

        if (fileLength > _limits.MaxFileBytes)
            return LimitMessage;

        try
        {
            using var stream = openPdf();
            return extractor.CountPages(stream) > _limits.MaxPages ? LimitMessage : null;
        }
        catch (InvalidPdfException ex)
        {
            return ex.Message;
        }
    }
}
