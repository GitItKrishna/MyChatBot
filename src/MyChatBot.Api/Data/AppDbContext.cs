using Microsoft.EntityFrameworkCore;

namespace MyChatBot.Api.Data;

/// <summary>
/// The DbContext is EF Core's "session" with the database: it knows our tables and
/// translates C# code (LINQ) into SQL. Everything that reads or writes data goes through it.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Embedding size. Amazon Titan Text Embeddings V2 produces 1024 numbers per text.</summary>
    public const int EmbeddingDimensions = 1024;

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Chunk> Chunks => Set<Chunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Makes the migration run "CREATE EXTENSION vector" - this switches pgvector on.
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Document>(e =>
        {
            e.ToTable("documents");
            e.Property(d => d.FileName).HasMaxLength(500);
            e.Property(d => d.S3Key).HasMaxLength(1024);
        });

        modelBuilder.Entity<Chunk>(e =>
        {
            e.ToTable("chunks");

            // Deleting a document also deletes its chunks.
            e.HasOne(c => c.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // vector(1024) is the pgvector column type with a fixed number of dimensions.
            e.Property(c => c.Embedding).HasColumnType($"vector({EmbeddingDimensions})");

            // The fast-search "table of contents" (HNSW index), using cosine distance.
            e.HasIndex(c => c.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            // Each chunk position is unique within a document.
            e.HasIndex(c => new { c.DocumentId, c.ChunkIndex }).IsUnique();
        });
    }
}
