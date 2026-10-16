namespace MyChatBot.Web;

/// <summary>
/// The UI's one gateway to the API. Pages call AskAsync(); this class does the HTTP.
/// Keeping HTTP code here (not inside the page) means the page only worries about display.
/// </summary>
public class ChatApiClient(HttpClient http)
{
    public async Task<QueryResponse> AskAsync(string question, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/query", new QueryRequest(question), ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QueryResponse>(ct)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }
}

// These mirror the API's request/response shapes (see QueryEndpoint.cs in MyChatBot.Api).
public record QueryRequest(string Question);
public record QueryResponse(string Answer, List<SourceReference> Sources);
public record SourceReference(string FileName, int Page, string Snippet);
