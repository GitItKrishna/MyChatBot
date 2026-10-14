var builder = DistributedApplication.CreateBuilder(args);

// Tell Aspire about our API. "api" is the name other services (like the Blazor UI later)
// will use to find it. Aspire picks the port and shows it in the dashboard.
builder.AddProject<Projects.MyChatBot_Api>("api")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
