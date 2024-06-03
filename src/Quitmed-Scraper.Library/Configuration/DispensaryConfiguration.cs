namespace Quitmed_Scraper.Library.Configuration;

public record DispensaryConfiguration
{
    public const string ConfigurationSection = "DispensaryConfiguration";
    public IEnumerable<Dispensary> Dispensaries { get; init; }

    public record Dispensary
    {
        public string Name { get; init; }
        public Guid Id { get; init; }
    }
}