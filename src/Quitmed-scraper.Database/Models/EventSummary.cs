namespace Quitmed_scraper.Database.Models;

public class EventSummary
{
    public Guid Id { get; set; }
    public Dispensary Dispensary { get; set; }
    public Product Product { get; set; }
    public string Message { get; set; }
    public DateTime TimestampUtc { get; set; }
}