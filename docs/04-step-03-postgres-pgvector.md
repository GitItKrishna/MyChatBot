# Step 03: PostgreSQL + pgvector with EF Core

**Goal:** persistent storage for document chunks and their embeddings, with the schema created at API startup.
**Issue:** #6 | **Branch:** `feature/03-postgres-pgvector`

## Concepts

- **pgvector** adds the `vector(n)` type and distance operators to PostgreSQL, so text and embeddings live in one database.
- **EF Core** maps entities to tables and translates LINQ to SQL. `Pgvector.EntityFrameworkCore` maps `Pgvector.Vector` to `vector`. `UseVector()` enables it on the Npgsql provider.
- **Migrations** are versioned schema changes. `DatabaseInitializer` (an `IHostedService`) calls `MigrateAsync()` at startup: on an empty database it enables the extension and creates tables and indexes; on a current one it does nothing.
- **Index:** HNSW with `vector_cosine_ops` for approximate nearest-neighbor search by cosine distance.
- **Dimension:** 1024, matching Titan Text Embeddings V2. Changing the model requires a migration.

## Schema

`documents`: `Id`, `FileName`, `S3Key`, `IngestedAt`

`chunks`: `Id`, `DocumentId` (FK, cascade delete), `PageNumber`, `ChunkIndex`, `Content`, `Embedding vector(1024)`

Indexes: unique `(DocumentId, ChunkIndex)`; HNSW on `Embedding`.

## Files

| File | Role |
|------|------|
| `Api/Data/Document.cs`, `Chunk.cs` | Entities |
| `Api/Data/AppDbContext.cs` | Model configuration: extension, `vector(1024)`, HNSW index, cascade delete |
| `Api/Data/Migrations/*InitialCreate*` | Generated migration |
| `Api/Data/DatabaseInitializer.cs` | Applies pending migrations at startup |
| `Api/Data/DesignTimeDbContextFactory.cs` | Context factory for `dotnet ef` only (placeholder connection string) |
| `Api/Endpoints/DbStatusEndpoint.cs` | `GET /api/db-status`: pgvector version and row counts |
| `Api/Program.cs` | Registers the DbContext with `UseVector()`, `EnrichNpgsqlDbContext`, the initializer |
| `AppHost/AppHost.cs` | Postgres container (`pgvector/pgvector:pg17`) with data volume; database `MyChatDB` |

## Wiring

1. `AppHost.cs` starts `pgvector/pgvector:pg17` (the stock `postgres` image lacks pgvector) and `AddDatabase("mychatdb", databaseName: "MyChatDB")`.
2. `.WithReference(chatDb)` injects the connection string `mychatdb` into the API; `.WaitFor(chatDb)` delays API start until the database is ready.
3. `Program.cs` reads `GetConnectionString("mychatdb")`; no credentials appear in code.
4. `DatabaseInitializer` runs the migration at startup.

Postgres folds unquoted identifiers to lowercase; quote `"MyChatDB"` in SQL.

## Commands

New migration:
```bash
dotnet ef migrations add NameOfChange --project src/MyChatBot.Api --output-dir Data/Migrations
```

Inspect the running database:
```bash
C=$(docker ps -q --filter ancestor=pgvector/pgvector:pg17)
docker exec -e PGPASSWORD="$(docker exec $C printenv POSTGRES_PASSWORD)" $C psql -U postgres -d MyChatDB -c '\dt'
```

## Verification
- `dotnet build` succeeds; migration generated.
- Through Aspire: tables created; `GET /api/db-status` returns `{"pgvectorVersion":"0.8.7","documents":0,"chunks":0}`.
- `psql` shows both tables, `vector(1024)` and the `hnsw ... vector_cosine_ops` index.
- A second run reuses the data volume and password without errors.

## Notes
- Startup migration suits development and small deployments; larger systems often run migrations as a separate deploy step.
