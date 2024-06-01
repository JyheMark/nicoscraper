using Akka.Hosting;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Configuration;
using Quitmed_Scraper.Library.Actors;
using Quitmed_Scraper.Library.Configuration;

namespace Quitmed_Scraper.WebApp.Extensions;

internal static class StartupExtensions
{
    public static void RegisterInternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        BindConfigurationModels(services, configuration);

        services.AddDbContext<QuitmedScraperDatabaseContext>();
        services.AddAkka("quitmed-scraper-actor-system", builder =>
        {
            builder
                .ConfigureLoggers(configBuilder => configBuilder.AddLoggerFactory())
                .WithActors((system, _, dependencies) =>
                {
                    system.ActorOf(dependencies.Props<OrchestrationActor>(), "scraper-orchestration-actor");
                });
        });
    }

    private static void BindConfigurationModels(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseConfiguration>()
            .Bind(configuration.GetSection(DatabaseConfiguration.ConfigurationSection));

        services
            .AddOptions<ScrapingScheduleConfiguration>()
            .Bind(configuration.GetSection(ScrapingScheduleConfiguration.ConfigurationSection));
    }
}