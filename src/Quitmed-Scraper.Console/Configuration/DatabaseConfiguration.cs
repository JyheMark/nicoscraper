using System.ComponentModel.DataAnnotations;

namespace Quitmed_Scraper.Console.Configuration;

internal record DatabaseConfiguration
{
    public const string ConfigurationSection = "Database";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
};