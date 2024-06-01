using System.ComponentModel.DataAnnotations;

namespace Quitmed_Scraper.Library.Configuration;

public record ScrapingScheduleConfiguration
{
    public const string ConfigurationSection = "Schedule";
    
    [Required(AllowEmptyStrings = false)]
    public TimeOnly ScrapeAt { get; init; }
}