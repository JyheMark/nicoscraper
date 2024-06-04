using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quitmed_scraper.Database.Configuration;
using Quitmed_scraper.Database.Models;

namespace Quitmed_scraper.Database;

public class QuitmedScraperDatabaseContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<ExecutionLog> ExecutionLogs { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Dispensary> Dispensaries { get; set; }
    public DbSet<EventSummary> EventSummaries { get; set; }

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
            entity.HasOne<Dispensary>(p => p.Dispensary).WithMany();
        });
        modelBuilder.Entity<Dispensary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.ScrapeUrl).IsRequired();
            entity.HasIndex(e => e.ScrapeUrl).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasMany<Product>(e => e.Products).WithOne(e => e.Dispensary);
        });
        modelBuilder.Entity<Dispensary>().HasData([
            new Dispensary()
            {
                Id = Guid.Parse("c123f55e-9d6b-4dd4-9754-11cddd50ef62"),
                Name = DispensaryNames.QuitMed,
                ScrapeUrl = "https://quitmed.com.au/collections/all"
            }
        ]);
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Vendor).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.InStock).IsRequired();
            entity.Property(e => e.IsArchived).IsRequired().HasDefaultValue(false);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasMany<HistoricalPricing>(e => e.PriceHistory).WithOne(e => e.Product);
        });
        modelBuilder.Entity<HistoricalPricing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).IsRequired();
        });
        modelBuilder.Entity<EventSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<Product>(e => e.Product).WithMany();
            entity.HasOne<Dispensary>(e => e.Dispensary).WithMany();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.TimestampUtc).IsRequired();
        });
    }
}