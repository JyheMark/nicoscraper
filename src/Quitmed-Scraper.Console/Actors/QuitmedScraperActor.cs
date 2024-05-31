using Akka.Actor;
using Akka.Event;

namespace Quitmed_Scraper.Console.Actors;

internal class QuitmedScraperActor : ReceiveActor
{
    private readonly ILoggingAdapter _logger;

    public QuitmedScraperActor()
    {
        _logger = Context.GetLogger();
        _logger.Info("Starting");

        Receive<BeginScraping>(_ =>
        {
            
        });
    }
    
    private record BeginScraping();
}