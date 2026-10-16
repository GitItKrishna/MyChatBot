# Step 01: Solution skeleton (Aspire + FastEndpoints)

**Goal:** a runnable .NET solution with an API and an Aspire orchestrator.
**Issue:** #1 | **Branch:** `feature/01-skeleton`

## Concepts

- **.NET Aspire** orchestrates the projects locally: starts them, injects connection strings and addresses, and provides a dashboard with logs, traces and health. It is a development tool; AWS deployment uses CDK (steps 10-13).
- **FastEndpoints** implements one class per endpoint. `Configure()` declares route and verb; `HandleAsync()` holds the logic.

## Projects

| Project | Role |
|---------|------|
| `MyChatBot.AppHost` | Aspire orchestrator; declares the resources to run |
| `MyChatBot.ServiceDefaults` | Shared logging, tracing, health checks, service discovery |
| `MyChatBot.Api` | Web API using FastEndpoints |

## Code

**`Api/Program.cs`**
- `AddServiceDefaults()` applies the shared Aspire configuration.
- `AddFastEndpoints()` / `UseFastEndpoints()` register all endpoint classes.
- `MapDefaultEndpoints()` exposes `/health` and `/alive` in Development.

**`Api/Endpoints/StatusEndpoint.cs`**
- `EndpointWithoutRequest<StatusResponse>`: no request body, typed response.
- `Get("/api/status")`, `AllowAnonymous()`, `Send.OkAsync(...)`.

**`AppHost/AppHost.cs`**
- `AddProject<Projects.MyChatBot_Api>("api")` registers the API under the name `api`.
- `WithHttpHealthCheck("/health")` ties the dashboard status to the health endpoint.

## Run

```bash
dotnet run --project src/MyChatBot.AppHost
```

API only:
```bash
dotnet run --project src/MyChatBot.Api
curl http://localhost:5205/api/status
```
Expected: `{"message":"MyChatBot API is running","time":"..."}`

## Verification
- `dotnet build`: 0 warnings, 0 errors.
- `GET /api/status` returns 200 with JSON; `GET /health` returns `Healthy`.

## Notes
- On .NET 10 the `dotnet new` Aspire templates need no separate workload.
- The port comes from `launchSettings.json` (5205), not `ASPNETCORE_URLS`.
