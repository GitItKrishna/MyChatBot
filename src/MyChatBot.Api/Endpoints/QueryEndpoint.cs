using FastEndpoints;

namespace MyChatBot.Api.Endpoints;

/// <summary>
/// POST /query - the question-answering endpoint.
/// In step 02 this is a PLACEHOLDER: it returns a canned answer so the UI has something to show.
/// Steps 07 and 08 replace the body with real retrieval + Nova answers. The request/response
/// shapes below stay the same, so the UI will not need to change.
/// </summary>
public class QueryEndpoint : Endpoint<QueryRequest, QueryResponse>
{
    public override void Configure()
    {
        Post("/query");
        AllowAnonymous();
    }

    public override async Task HandleAsync(QueryRequest req, CancellationToken ct)
    {
        var response = new QueryResponse(
            Answer: $"(placeholder) You asked: \"{req.Question}\". Real answers arrive in step 08.",
            Sources:
            [
                new SourceReference("sample-document.pdf", 1, "Example passage that a real answer would be based on."),
            ]);

        await Send.OkAsync(response, ct);
    }
}

/// <summary>What the browser sends: just the user's question.</summary>
public record QueryRequest(string Question);

/// <summary>What we send back: the answer plus where it came from.</summary>
public record QueryResponse(string Answer, List<SourceReference> Sources);

/// <summary>One "citation": which file, which page, and the matching text snippet.</summary>
public record SourceReference(string FileName, int Page, string Snippet);
