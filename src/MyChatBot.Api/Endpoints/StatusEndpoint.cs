using FastEndpoints;

namespace MyChatBot.Api.Endpoints;

/// <summary>
/// Our first FastEndpoints endpoint: GET /api/status.
/// One class = one endpoint. Later steps add /ingest and /query the same way.
/// </summary>
public class StatusEndpoint : EndpointWithoutRequest<StatusResponse>
{
    public override void Configure()
    {
        Get("/api/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new StatusResponse("MyChatBot API is running", DateTimeOffset.UtcNow), ct);
    }
}

public record StatusResponse(string Message, DateTimeOffset Time);
