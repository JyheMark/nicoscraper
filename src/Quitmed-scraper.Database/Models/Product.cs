namespace Quitmed_scraper.Database.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool InStock { get; set; }
    public Dispensary Dispensary { get; set; } = null!;
    public List<HistoricalPricing> PriceHistory { get; set; } = null!;
}