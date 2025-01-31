﻿using System.ComponentModel.DataAnnotations;

namespace Quitmed_scraper.Database.Configuration;

public record DatabaseConfiguration
{
    public const string ConfigurationSection = "Database";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
};