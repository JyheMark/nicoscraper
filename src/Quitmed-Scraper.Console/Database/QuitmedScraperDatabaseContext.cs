using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quitmed_Scraper.Console.Configuration;
using Quitmed_Scraper.Console.Database.Models;

namespace Quitmed_Scraper.Console.Database;

internal class QuitmedScraperDatabaseContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<ExecutionLog> ExecutionLogs { get; set; }

    public QuitmedScraperDatabaseContext(IOptions<DatabaseConfiguration> databaseConfiguration)
    {
        _connectionString = databaseConfiguration.Value.ConnectionString;
        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExecutionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.StartTimeUtc).IsRequired();
            entity.Property(e => e.EndTimeUtc).IsRequired();
        });
    }
}