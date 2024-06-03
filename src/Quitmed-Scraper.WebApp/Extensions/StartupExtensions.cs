using Akka.Hosting;
using MudBlazor.Services;
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

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

        services.AddSingleton(httpClient);
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
        services.AddMudServices();
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