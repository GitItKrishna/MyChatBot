# Step 02: Blazor chat UI

**Goal:** a chat page that sends a question to the API and displays the answer with sources.
**Issue:** #4 | **Branch:** `feature/02-blazor-ui`

## Concepts

- **Blazor Server.** Components render on the server; UI events and DOM updates travel over a SignalR connection. Every UI event is a server round trip.
- **Service discovery.** The UI registers its `HttpClient` with `https+http://api`. Aspire resolves `api` to the API's actual address, provided `AppHost.cs` calls `.WithReference(api)`. No ports are hard-coded.
- **Contract first.** `POST /query` is a placeholder returning a canned answer. The request/response shapes are final, so steps 07-08 replace only the implementation:
  - Request: `{ "question": string }`
  - Response: `{ "answer": string, "sources": [{ "fileName": string, "page": int, "snippet": string }] }`

## Files

| File | Role |
|------|------|
| `Api/Endpoints/QueryEndpoint.cs` | Placeholder `POST /query` |
| `Web/ChatApiClient.cs` | Typed HTTP client for the API |
| `Web/Components/Pages/Chat.razor` (+ `.cs`, `.css`) | Chat page: markup, code-behind, scoped styles |
| `Web/Program.cs` | Aspire defaults, `ChatApiClient` registration |
| `AppHost/AppHost.cs` | Adds `web` with `WithReference(api)` and `WaitFor(api)` |

## Chat page flow
1. `@bind` with `oninput` keeps `_question` current.
2. Enter or Send calls `SendAsync()`, which ignores empty input.
3. It appends the user message, calls `ChatApiClient.AskAsync()`, then appends the reply.
4. `_isThinking` shows a pending indicator and disables the form while waiting.
5. `HttpRequestException` is caught and shown as an error message.

## Defect found and fixed
The Send button was disabled while the input was empty. In Blazor Server, the "input has text" state reaches the client only after a server round trip, so a fast Enter press landed on a still-disabled button and was dropped. Fix: disable the button only while a request is in flight; `SendAsync` already guards against empty input. Validate in code rather than relying on UI state updated through a round trip.

## Run

```bash
dotnet run --project src/MyChatBot.AppHost
```
Open the `web` endpoint from the dashboard.

## Verification
- `dotnet build` succeeds.
- Through Aspire: a question flows UI -> API -> UI; the placeholder answer and source render; Enter submits.
