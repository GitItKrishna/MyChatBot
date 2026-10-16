var builder = DistributedApplication.CreateBuilder(args);

// Tell Aspire about our API. "api" is the name other services (like the Blazor UI)
// use to find it. Aspire picks the port and shows it in the dashboard.
var api = builder.AddProject<Projects.MyChatBot_Api>("api")
    .WithHttpHealthCheck("/health");

// The Blazor UI. WithReference(api) gives it the API's address under the name "api".
// WaitFor(api) makes Aspire start the UI only after the API reports healthy.
builder.AddProject<Projects.MyChatBot_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
