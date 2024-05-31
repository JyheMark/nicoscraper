namespace Quitmed_scraper.Database.Models;

public class ExecutionLog
{
    public Guid Id { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public Dispensary Dispensary { get; set; }
}