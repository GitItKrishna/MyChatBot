using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MyChatBot.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Aspire plumbing: logging, tracing, health checks, service discovery.
builder.AddServiceDefaults();

// Database. "mychatdb" is the resource name from AppHost.cs; Aspire puts the real connection string
// in configuration under that name. UseVector() teaches EF Core about pgvector's vector type.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("mychatdb"), npgsql => npgsql.UseVector()));

// Adds Aspire extras to the DbContext: a health check, tracing and retry on temporary failures.
builder.EnrichNpgsqlDbContext<AppDbContext>();

// Creates/updates the tables each time the API starts.
builder.Services.AddHostedService<DatabaseInitializer>();

// FastEndpoints scans this assembly for classes that inherit Endpoint<,>.
builder.Services.AddFastEndpoints();

var app = builder.Build();

// Registers every FastEndpoints endpoint found above.
app.UseFastEndpoints();

// Aspire's /health and /alive endpoints (Development only by default).
app.MapDefaultEndpoints();

app.Run();
