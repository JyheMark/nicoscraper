using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quitmed_Scraper.Console.Actors;
using Quitmed_Scraper.Console.Configuration;
using Quitmed_Scraper.Console.Database;

namespace Quitmed_Scraper.Console.Extensions;

internal static class StartupExtensions
{
    public static void AddJsonFiles(this IConfigurationBuilder configuration)
    {
        configuration.AddJsonFile("appsettings.json");
        configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
    }

    public static void BindConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseConfiguration>()
            .Bind(configuration.GetSection(DatabaseConfiguration.ConfigurationSection));
    }

    public static void RegisterInternalServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.BindConfigurationOptions(configuration);

        serviceCollection.AddSingleton<HttpClient>();
        serviceCollection.AddLogging();
        serviceCollection.AddDbContext<QuitmedScraperDatabaseContext>();
        serviceCollection.AddAkka("scraper-actor-system", builder =>
        {
            builder
                .ConfigureLoggers(configBuilder => configBuilder.AddLoggerFactory())
                .WithActors((system, _, dependencies) =>
                {
                    system.ActorOf(dependencies.Props<QuitmedScraperActor>(), "quitmed-scraper-actor");
                });
        });
    }
}