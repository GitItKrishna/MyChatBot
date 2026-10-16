namespace MyChatBot.Api.Pdf;

/// <summary>
/// Upload limits for PDFs, read from the "PdfLimits" section of appsettings.json.
/// They keep one oversized file from using lots of memory, time and (later) Bedrock embedding cost.
/// </summary>
public class PdfLimitsOptions
{
    public const string SectionName = "PdfLimits";

    /// <summary>Largest accepted file in bytes. Default 2 MB.</summary>
    public long MaxFileBytes { get; set; } = 2 * 1024 * 1024;

    /// <summary>Most pages accepted in one PDF.</summary>
    public int MaxPages { get; set; } = 5;
}
