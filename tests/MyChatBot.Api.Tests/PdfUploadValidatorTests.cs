using Microsoft.Extensions.Options;
using MyChatBot.Api.Pdf;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace MyChatBot.Api.Tests;

public class PdfUploadValidatorTests
{
    private static PdfUploadValidator CreateValidator(long maxBytes = 2 * 1024 * 1024, int maxPages = 5) =>
        new(Options.Create(new PdfLimitsOptions { MaxFileBytes = maxBytes, MaxPages = maxPages }), new PdfPigTextExtractor());

    private static byte[] BuildPdf(int pageCount)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        for (var i = 1; i <= pageCount; i++)
            builder.AddPage(595, 842).AddText($"Page {i}", 12, new PdfPoint(50, 780), font);
        return builder.Build();
    }

    private static string? Validate(PdfUploadValidator validator, byte[] pdf, long? reportedLength = null) =>
        validator.Validate(reportedLength ?? pdf.Length, () => new MemoryStream(pdf));

    [Theory]
    [InlineData(1)]
    [InlineData(5)] // exactly at the limit is allowed
    public void Pdf_within_the_page_limit_is_accepted(int pages)
    {
        Assert.Null(Validate(CreateValidator(), BuildPdf(pages)));
    }

    [Fact]
    public void Pdf_over_the_page_limit_is_rejected_with_the_limit_message()
    {
        var validator = CreateValidator();

        var message = Validate(validator, BuildPdf(6));

        Assert.Equal(validator.LimitMessage, message);
    }

    [Fact]
    public void File_over_the_size_limit_is_rejected_without_opening_it()
    {
        var validator = CreateValidator(maxBytes: 2 * 1024 * 1024);
        var opened = false;

        var message = validator.Validate(2 * 1024 * 1024 + 1, () => { opened = true; return Stream.Null; });

        Assert.Equal(validator.LimitMessage, message);
        Assert.False(opened, "an oversized file must be rejected on size alone, before reading it");
    }

    [Fact]
    public void File_of_exactly_the_size_limit_is_accepted()
    {
        var pdf = BuildPdf(1);

        Assert.Null(Validate(CreateValidator(maxBytes: pdf.Length), pdf));
    }

    [Fact]
    public void Limit_message_states_both_limits()
    {
        var message = CreateValidator().LimitMessage;

        Assert.Contains("2 MB", message);
        Assert.Contains("5 pages", message);
    }

    [Fact]
    public void Empty_upload_is_rejected()
    {
        Assert.NotNull(CreateValidator().Validate(0, () => Stream.Null));
    }

    [Fact]
    public void Non_pdf_content_is_rejected()
    {
        var message = Validate(CreateValidator(), "not a pdf"u8.ToArray());

        Assert.Equal("The file could not be read as a PDF.", message);
    }
}
