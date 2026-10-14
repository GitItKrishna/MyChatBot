# Step 01: Solution skeleton (Aspire + FastEndpoints)

**Goal:** a running .NET solution with an API and an orchestrator, so every later feature has a home.
**Issue:** #1 | **Branch:** `feature/01-skeleton`

## Concepts in plain English

### What is a "solution" and a "project"?
A **project** (`.csproj`) is one buildable unit, like one app. A **solution** (`MyChatBot.slnx`) is a folder-like file that groups projects so you can build them together.

### What is .NET Aspire?
Real apps are several pieces: an API, a UI, a database. Starting each one by hand, with the right ports and connection strings, is tedious. **Aspire** is a developer tool that:
- starts all pieces with one command,
- wires them together (the UI finds the API by the name `"api"`, no hard-coded ports),
- shows a **dashboard** with logs, traces and health for everything.

Aspire is for **local development and orchestration**. It does not deploy to AWS by itself; in steps 10-13 we deploy with CDK.

### What is FastEndpoints?
The normal ASP.NET way mixes many actions into big "controller" classes. FastEndpoints uses **one class per endpoint**. Each class says its URL and verb in `Configure()` and its logic in `HandleAsync()`. Small, easy to find, easy to test. Our `/ingest` and `/query` will each be one class.

## The three projects

| Project | Role | Analogy |
|---------|------|---------|
| `MyChatBot.AppHost` | The Aspire orchestrator: lists what to run | The conductor |
| `MyChatBot.ServiceDefaults` | Shared setup: logging, health checks, tracing | The shared house rules |
| `MyChatBot.Api` | The web API with FastEndpoints | The first musician |

## Walkthrough of the code

**`src/MyChatBot.Api/Program.cs`**
- `AddServiceDefaults()`: plugs in the Aspire "house rules" (health checks, telemetry).
- `AddFastEndpoints()` + `UseFastEndpoints()`: find and register every endpoint class.
- `MapDefaultEndpoints()`: exposes `/health` and `/alive` (only in Development, for safety).

**`src/MyChatBot.Api/Endpoints/StatusEndpoint.cs`**
- Inherits `EndpointWithoutRequest<StatusResponse>`: "no input, returns a `StatusResponse`".
- `Get("/api/status")`: the URL and HTTP verb.
- `AllowAnonymous()`: no login required (we add auth later if needed).
- `Send.OkAsync(...)`: returns HTTP 200 with JSON.

**`src/MyChatBot.AppHost/AppHost.cs`**
- `AddProject<Projects.MyChatBot_Api>("api")`: "run the API project and call it `api`".
- `WithHttpHealthCheck("/health")`: the dashboard shows green only when `/health` answers.

## How to run it

```bash
dotnet run --project src/MyChatBot.AppHost
```
The terminal prints a dashboard URL. Open it, click the `api` endpoint link, and add `/api/status`.

Or without Aspire, just the API:
```bash
dotnet run --project src/MyChatBot.Api
curl http://localhost:5205/api/status
```
Expected: `{"message":"MyChatBot API is running","time":"..."}`

## What we verified
- `dotnet build`: 0 warnings, 0 errors.
- `GET /api/status` returns 200 with JSON; `GET /health` returns `Healthy`.

## Things that surprised us
- The `dotnet new` templates come with Aspire installed; no separate workload is needed on .NET 10.
- `launchSettings.json` picks the port (5205), not the `ASPNETCORE_URLS` variable.

## Next
Step 02: a Blazor chat page that calls this API through Aspire service discovery.
