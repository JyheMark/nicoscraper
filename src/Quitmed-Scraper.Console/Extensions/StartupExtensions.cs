using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quitmed_Scraper.Console.Actors;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Configuration;

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

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        serviceCollection.AddSingleton(httpClient);
        
        serviceCollection.AddLogging();
        serviceCollection.AddDbContext<QuitmedScraperDatabaseContext>();
        serviceCollection.AddAkka("scraper-actor-system", builder =>
        {
            builder
                .ConfigureLoggers(configBuilder => configBuilder.AddLoggerFactory())
                .WithActors((system, _, dependencies) =>
                {
                    system.ActorOf(dependencies.Props<OrchestrationActor>(), "scraper-orchestration-actor");
                });
        });
    }
}