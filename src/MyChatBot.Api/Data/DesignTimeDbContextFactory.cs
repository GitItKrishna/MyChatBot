using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyChatBot.Api.Data;

/// <summary>
/// Only used by the "dotnet ef migrations add ..." command-line tool, never at runtime.
/// The tool needs to build an AppDbContext, but outside Aspire there is no real connection
/// string. Creating a migration does not connect to a database, so a placeholder is fine.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=MyChatDB", npgsql => npgsql.UseVector())
            .Options;

        return new AppDbContext(options);
    }
}
