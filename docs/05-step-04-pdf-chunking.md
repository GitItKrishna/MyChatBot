# Step 04: PDF extraction and chunking

**Goal:** turn a PDF into page-aware text chunks ready for embedding.
**Branch:** `feature/04-pdf-chunking`

## Concepts

- **Page-aware extraction.** Text is extracted per page so every chunk keeps an exact page number for citations. A chunk never spans two pages.
- **Chunking.** Whole documents match too coarsely and single sentences lose context, so text is cut into pieces of at most `MaxChars` (default 800).
- **Overlap.** Each chunk begins `OverlapChars` (default 150) before the previous one ended, so a sentence cut at a boundary is still whole in one chunk.
- **Break preference.** For each chunk the splitter looks in the second half of the allowed window for, in order: a paragraph break, a sentence end, whitespace, then a hard cut. Searching only the second half prevents tiny chunks.
- **PdfPig** is a pure .NET PDF reader with no native dependencies, so it runs unchanged in the Linux container used for Fargate. `ContentOrderTextExtractor` orders words in reading order.

## Files

| File | Role |
|------|------|
| `Api/Pdf/PageText.cs` | `record PageText(PageNumber, Text)`, 1-based page numbers |
| `Api/Pdf/IPdfTextExtractor.cs` | Extraction abstraction; throws `InvalidPdfException` for unreadable input |
| `Api/Pdf/PdfPigTextExtractor.cs` | PdfPig implementation |
| `Api/Pdf/PdfLimitsOptions.cs` | `MaxFileBytes` (2 MB) and `MaxPages` (5), bound from the `PdfLimits` config section |
| `Api/Pdf/PdfUploadValidator.cs` | Checks an upload against the limits; returns null or the message to show |
| `Api/Chunking/ChunkingOptions.cs` | `MaxChars`, `OverlapChars`, bound from the `Chunking` config section |
| `Api/Chunking/TextChunk.cs` | `record TextChunk(PageNumber, ChunkIndex, Content)`; matches the `Chunk` entity |
| `Api/Chunking/ITextChunker.cs`, `TextChunker.cs` | The chunking algorithm |
| `Api/Endpoints/ChunkPreviewEndpoint.cs` | `POST /api/chunk-preview`: upload a PDF, see the chunks. Nothing is stored. |
| `Api/Program.cs`, `appsettings.json` | Registration and default settings |
| `tests/MyChatBot.Api.Tests/` | 25 xUnit tests for the chunker, extractor and upload validator |
| `samples/acme-store-policy.pdf` | Realistic sample document reused in later steps |

## Algorithm details

1. `Normalize`: unify line endings, collapse whitespace, turn single line breaks into spaces (PDFs break lines mid-sentence), keep blank lines as paragraph breaks.
2. Window of `MaxChars` from `start`; unless the text ends inside it, find the break point as described above.
3. Emit the trimmed chunk, then `start = end - OverlapChars`, advanced to the next word start so a chunk never begins mid-word.
4. `ChunkIndex` increases across the whole document, matching the unique `(DocumentId, ChunkIndex)` index from step 03.

Option validation: `MaxChars >= 100` and `0 <= OverlapChars < MaxChars / 2`. The second rule guarantees each chunk advances by more than half its length, so the loop always terminates.

## Upload limits

A PDF over 2 MB or over 5 pages is rejected as a whole: nothing is extracted, chunked or stored. The response is HTTP 400 with:

```
File must be no larger than 2 MB and have no more than 5 pages.
```

- Order of checks: empty file, size, then page count (`IPdfTextExtractor.CountPages`, which does not extract text), so an oversized file is refused before it is parsed.
- Limits are inclusive: exactly 2 MB and exactly 5 pages are accepted.
- Change them in `src/MyChatBot.Api/appsettings.json` under `PdfLimits`. The message is built from the configured values.
- `PdfUploadValidator` is shared code; `/ingest` (step 06) will reuse it.

## Run

```bash
dotnet test
```

```bash
dotnet run --project src/MyChatBot.AppHost
```

Then call the API's address from the Aspire dashboard:

```bash
curl -F "file=@samples/acme-store-policy.pdf" http://localhost:<api-port>/api/chunk-preview
```

Response fields: `fileName`, `pageCount`, `pagesWithoutText`, `chunkCount`, and `chunks[]` with `page`, `index`, `length`, `content`.

Tune `Chunking:MaxChars` and `Chunking:OverlapChars` in `src/MyChatBot.Api/appsettings.json` and compare the output.

## Verification
- `dotnet test`: 25 passed.
- Sample PDF (2 pages): 8 chunks, longest 796 characters, page numbers preserved, overlap visible between consecutive chunks.
- 9-page PDF and a 2.2 MB file are both rejected with the limit message (HTTP 400); `documents` and `chunks` counts stay 0.
- Invalid PDF returns 400 with a message; a request without a multipart body returns 415.

## Limitations
- **Scanned PDFs.** Pages that are images contain no text and come back empty (reported as `pagesWithoutText`). They would need OCR, for example Amazon Textract.
- **Paragraph breaks.** Many PDFs encode lines, not paragraphs, so paragraph breaks are often absent and the splitter falls back to sentence ends.
- **Tables and columns.** Reading order is a heuristic; multi-column layouts and tables can interleave.
- **Hyphenation.** Words hyphenated across lines are not rejoined.
- **Character-based sizes.** Limits are characters, not tokens; the embedding model's token limit is checked in step 05.

## Test PDF caveat
macOS `cupsfilter` wraps lines at a fixed width and splits words (for example "refu / nd"), which produces words with stray spaces after extraction. The extractor was correct; the generated file was not. Wrap text with `fold -s` before converting.
