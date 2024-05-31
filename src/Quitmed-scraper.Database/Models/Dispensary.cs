namespace Quitmed_scraper.Database.Models;

public class Dispensary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScrapeUrl { get; set; } = string.Empty;
    public List<Product>? Products { get; set; }
}