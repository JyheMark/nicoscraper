namespace Quitmed_Scraper.Console.Database.Models;

internal class ExecutionLog
{
    public Guid Id { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
}