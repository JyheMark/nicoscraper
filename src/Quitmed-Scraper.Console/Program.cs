using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quitmed_Scraper.Console.Extensions;

IHost? app = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(builder => builder.AddJsonFiles())
    .ConfigureServices((builder, services) => services.RegisterInternalServices(builder.Configuration))
    .Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application terminated unexpectedly");
}