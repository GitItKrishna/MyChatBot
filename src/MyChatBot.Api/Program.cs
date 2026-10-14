using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Aspire plumbing: logging, tracing, health checks, service discovery.
builder.AddServiceDefaults();

// FastEndpoints scans this assembly for classes that inherit Endpoint<,>.
builder.Services.AddFastEndpoints();

var app = builder.Build();

// Registers every FastEndpoints endpoint found above.
app.UseFastEndpoints();

// Aspire's /health and /alive endpoints (Development only by default).
app.MapDefaultEndpoints();

app.Run();
