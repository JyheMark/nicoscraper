using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Configuration;
using Quitmed_Scraper.Database.Seeder;

var app = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json");
        configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddOptions<DatabaseConfiguration>().Bind(configuration.GetSection(DatabaseConfiguration.ConfigurationSection));
        services.AddOptions<SeedDataConfiguration>().Bind(configuration.GetSection(SeedDataConfiguration.ConfigurationSection));
        
        services.AddDbContext<QuitmedScraperDatabaseContext>();
    })
    .Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting seed data process");
    using var serviceScope = app.Services.CreateScope();
    var seedDataConfig = serviceScope.ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>().Value;

    if (!seedDataConfig.Dispensaries.Any())
        return;
    
    logger.LogInformation($"Adding {seedDataConfig.Dispensaries.Count()} dispensaries to database");
    
    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();
    await dbContext.Dispensaries.AddRangeAsync(seedDataConfig.Dispensaries);
    await dbContext.SaveChangesAsync();
    
    logger.LogInformation("Seed data successfully added to database");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application terminated unexpectedly");
}