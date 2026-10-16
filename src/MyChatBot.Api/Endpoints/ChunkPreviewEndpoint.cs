using FastEndpoints;
using MyChatBot.Api.Chunking;
using MyChatBot.Api.Pdf;

namespace MyChatBot.Api.Endpoints;

/// <summary>
/// POST /api/chunk-preview - upload a PDF and see exactly how it would be cut into chunks.
/// Nothing is stored and no AWS service is called. It exists so you can SEE step 04's output
/// and experiment with chunk size and overlap. The real POST /ingest arrives in step 06.
/// </summary>
public class ChunkPreviewEndpoint(IPdfTextExtractor extractor, ITextChunker chunker, PdfUploadValidator validator)
    : Endpoint<ChunkPreviewRequest, ChunkPreviewResponse>
{
    public override void Configure()
    {
        Post("/api/chunk-preview");
        AllowAnonymous();
        AllowFileUploads(); // the request is multipart/form-data, not JSON
        Description(d => d.Accepts<ChunkPreviewRequest>("multipart/form-data"));
    }

    public override async Task HandleAsync(ChunkPreviewRequest req, CancellationToken ct)
    {
        // Size and page-count limits (from appsettings.json). A file over either limit is
        // rejected here, before any text is extracted, and nothing is kept.
        var problem = req.File is null ? "Upload a PDF in the 'file' form field."
            : validator.Validate(req.File.Length, req.File.OpenReadStream);
        if (problem is not null)
            ThrowError(r => r.File, problem);

        IReadOnlyList<PageText> pages;
        try
        {
            await using var stream = req.File!.OpenReadStream();
            pages = extractor.ExtractPages(stream);
        }
        catch (InvalidPdfException ex)
        {
            ThrowError(r => r.File, ex.Message);
            return;
        }

        var chunks = chunker.Chunk(pages);

        await Send.OkAsync(new ChunkPreviewResponse(
            FileName: req.File!.FileName,
            PageCount: pages.Count,
            PagesWithoutText: pages.Count(p => string.IsNullOrWhiteSpace(p.Text)),
            ChunkCount: chunks.Count,
            Chunks: chunks.Select(c => new ChunkPreviewItem(c.PageNumber, c.ChunkIndex, c.Content.Length, c.Content)).ToList()),
            ct);
    }
}

public class ChunkPreviewRequest
{
    public IFormFile File { get; set; } = null!;
}

public record ChunkPreviewResponse(
    string FileName,
    int PageCount,
    int PagesWithoutText,
    int ChunkCount,
    List<ChunkPreviewItem> Chunks);

public record ChunkPreviewItem(int Page, int Index, int Length, string Content);
