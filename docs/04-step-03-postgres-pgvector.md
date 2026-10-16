# Step 03: PostgreSQL + pgvector with Entity Framework Core

**Goal:** give the app a memory: a database that can store text chunks and their meaning-vectors, created automatically when the API starts.
**Branch:** `feature/03-postgres-pgvector`

## Concepts in plain English

### Database, table, row, column
A **database** is organized storage. A **table** is like a spreadsheet tab with fixed columns. A **row** is one record. We have two tables: `documents` (one row per PDF) and `chunks` (one row per piece of text).

### PostgreSQL and pgvector
**PostgreSQL** is a standard database. **pgvector** is an extension that adds a `vector` column type and "find the nearest vectors" search. One database holds both the text and its meaning-vector, so no separate vector database is needed.

### What is a vector, again?
A list of numbers (1024 of them here) that captures the *meaning* of a text. Texts with similar meaning have vectors that point in similar directions. Step 05 produces them with Amazon Titan.

### Entity Framework Core (EF Core)
EF Core is a translator between C# and SQL. You write ordinary C# classes and queries; EF Core writes the SQL.
- **Entity**: a C# class that maps to a table (`Document`, `Chunk`).
- **DbContext**: the session with the database; it exposes tables as `DbSet<T>` properties.
- **LINQ**: C# query syntax, e.g. `db.Chunks.CountAsync()` becomes `SELECT COUNT(*) FROM chunks`.
- **Migration**: a generated, versioned script that changes the database structure. Like "git commits" for your tables.

### Bootstrapping the schema
When the API starts, `DatabaseInitializer` asks "which migrations have not been applied yet?" and applies them. On an empty database: enable pgvector, create tables, create the index. On an up-to-date one: nothing happens. Safe to run on every start, and we will reuse the idea on RDS in AWS.

### HNSW index
Without an index, search compares the question to every chunk. An **HNSW** index is like a smart table of contents: it jumps near the right neighborhood of vectors. It trades a tiny bit of exactness for a big speed gain. We use **cosine distance** (`vector_cosine_ops`), which compares direction, not length.

## The new files

| File | Role |
|------|------|
| `Api/Data/Document.cs` | Entity: one PDF (`documents` table) |
| `Api/Data/Chunk.cs` | Entity: one text piece + its embedding (`chunks` table) |
| `Api/Data/AppDbContext.cs` | Tells EF Core about the tables, the `vector(1024)` column, the HNSW index and the cascade delete |
| `Api/Data/Migrations/*InitialCreate*` | Generated script: create extension, tables, indexes |
| `Api/Data/DatabaseInitializer.cs` | Runs pending migrations at API startup |
| `Api/Data/DesignTimeDbContextFactory.cs` | Lets the `dotnet ef` command-line tool build a context (never used at runtime) |
| `Api/Endpoints/DbStatusEndpoint.cs` | `GET /api/db-status`: shows pgvector version and row counts |
| `Api/Program.cs` | Registers the DbContext (with `UseVector()`), the initializer and Aspire extras |
| `AppHost/AppHost.cs` | Starts Postgres (pgvector image) in Docker and wires it to the API |

## How the pieces connect
1. `AppHost.cs` starts a **pgvector/pgvector:pg17** Docker container and creates a database called `MyChatDB`.
2. `.WithReference(chatDb)` hands the API the connection string under the resource name `mychatdb`; `.WaitFor(chatDb)` delays the API until the database is ready.
3. `Program.cs` reads that connection string, so no password is ever written in our code.
4. On startup `DatabaseInitializer` runs the migration, then the API serves requests.

## Commands to know

Create a new migration after changing an entity:
```bash
dotnet ef migrations add NameOfChange --project src/MyChatBot.Api --output-dir Data/Migrations
```

Look inside the running database (while the app is running):
```bash
C=$(docker ps -q --filter ancestor=pgvector/pgvector:pg17)
docker exec -e PGPASSWORD="$(docker exec $C printenv POSTGRES_PASSWORD)" $C psql -U postgres -d MyChatDB -c '\dt'
```

## What we verified
- `dotnet build` succeeds; migration generated.
- Through Aspire: Postgres container starts, tables are created, `GET /api/db-status` returns `{"pgvectorVersion":"0.8.7","documents":0,"chunks":0}`.
- `psql` shows `documents`, `chunks`, `vector(1024)` column and the `hnsw ... vector_cosine_ops` index.
- A second run reuses the same data volume and password without errors.

## Things to remember
- The plain `postgres` Docker image does not include pgvector; we use `pgvector/pgvector`.
- The vector dimension (1024) must match the embedding model. Changing the model later means a migration.
- The API's startup migration is convenient for learning and small apps; larger teams often run migrations as a separate deploy step.

## Next
Step 04: read a PDF page by page and cut it into chunks.
