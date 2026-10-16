var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL in a Docker container. The standard postgres image does NOT include pgvector,
// so we swap in the "pgvector/pgvector" image, which is Postgres plus the vector extension.
// WithDataVolume keeps the data between runs (otherwise it vanishes when the container stops).
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector")
    .WithImageTag("pg17")
    .WithDataVolume();

// A database inside that server. "mychatdb" is the resource name the API uses to find it
// (it becomes the connection string name); "MyChatDB" is the real database name in Postgres.
var chatDb = postgres.AddDatabase("mychatdb", databaseName: "MyChatDB");

// Tell Aspire about our API. "api" is the name other services (like the Blazor UI)
// use to find it. Aspire picks the port and shows it in the dashboard.
var api = builder.AddProject<Projects.MyChatBot_Api>("api")
    .WithHttpHealthCheck("/health")
    .WithReference(chatDb)   // gives the API the connection string for "mychatdb"
    .WaitFor(chatDb);        // start the API only after the database is ready

// The Blazor UI. WithReference(api) gives it the API's address under the name "api".
// WaitFor(api) makes Aspire start the UI only after the API reports healthy.
builder.AddProject<Projects.MyChatBot_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
