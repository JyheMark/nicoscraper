using Quitmed_scraper.Database.Models;

namespace Quitmed_Scraper.Database.Seeder;

internal class SeedDataConfiguration
{
    public const string ConfigurationSection = "SeedData";
    public IEnumerable<Dispensary> Dispensaries { get; set; }
}