namespace Quitmed_scraper.Database.Models;

public class HistoricalPricing
{
    public Guid Id { get; set; }
    public int Price { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}