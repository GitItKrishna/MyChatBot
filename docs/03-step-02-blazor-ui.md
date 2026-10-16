# Step 02: Blazor chat UI

**Goal:** a chat page in the browser that sends a question to our API and shows the answer with its sources.
**Issue:** #4 | **Branch:** `feature/02-blazor-ui`

## Concepts in plain English

### What is Blazor?
Normally web pages are built with JavaScript. **Blazor** lets you build them with C# instead. A page is a `.razor` file: HTML with C# mixed in.

We use **Blazor Server** mode. The page runs on the server, and a live connection (SignalR/WebSocket) sends clicks and keystrokes to the server and sends back only the changes to the screen. Simple to build, but every keystroke is a round trip to the server, which matters for the bug we hit below.

### How does the UI find the API? (service discovery)
The API's port is chosen by Aspire and changes each run, so we never hard-code it. In `Program.cs` the UI uses the address `https+http://api`. That is not a real address. Aspire sees the name `api` and swaps in the real one. This works because in `AppHost.cs` we wrote `.WithReference(api)`.

### Why is there a placeholder `/query` endpoint?
We decide the **contract** now: the UI sends `{ question }`, the API replies `{ answer, sources: [{fileName, page, snippet}] }`. Steps 07 and 08 fill in the real logic behind the same shapes, so the UI never needs to change. Building UI first against a fake backend is a common, useful trick.

## What each new file does

| File | Role |
|------|------|
| `MyChatBot.Api/Endpoints/QueryEndpoint.cs` | Placeholder `POST /query` returning a canned answer and one fake source |
| `MyChatBot.Web/ChatApiClient.cs` | The UI's only code that talks HTTP to the API |
| `MyChatBot.Web/Components/Pages/Chat.razor` | The chat page: message list, input box, Send button |
| `MyChatBot.Web/Components/Pages/Chat.razor.css` | Styles for just this page (blue user bubbles, grey bot bubbles) |
| `MyChatBot.Web/Program.cs` | Adds Aspire defaults and registers the API client |
| `MyChatBot.AppHost/AppHost.cs` | Adds the `web` project, gives it the `api` reference, starts it after the API is healthy |

## How the Chat page works
1. You type; `@bind` copies the text into `_question`.
2. Pressing Enter or clicking Send runs `SendAsync()`.
3. It adds your message to `_messages`, calls `Api.AskAsync()`, then adds the bot's reply.
4. Blazor redraws the list. `_isThinking` shows "Thinking..." and disables the input meanwhile.
5. If the API is down, we catch the error and show a friendly message instead of crashing.

## Bug we hit (worth remembering)
The Send button was disabled while the text box was empty. Pressing Enter right after typing did nothing, because in Blazor Server the "box now has text" news takes a trip to the server and back, and the button was still disabled when Enter landed. **Fix:** only disable the button while waiting for an answer; `SendAsync` already ignores empty questions. Lesson: do not lean on UI state that updates through a server round trip for input validation; validate in code.

## How to run it
```bash
dotnet run --project src/MyChatBot.AppHost
```
Open the dashboard URL printed in the terminal, find the `web` row, click its link, and ask a question.

## What we verified
- `dotnet build`: succeeds.
- Through Aspire, the page loads, a question goes UI -> API -> back, the placeholder answer and source show, and Enter works.

## Next
Step 03: PostgreSQL with pgvector in Docker through Aspire, and a schema created on startup.
