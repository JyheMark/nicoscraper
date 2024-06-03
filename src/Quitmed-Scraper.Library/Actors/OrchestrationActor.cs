using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Models;
using Quitmed_Scraper.Library.Actors.Messages;
using Quitmed_Scraper.Library.Configuration;

namespace Quitmed_Scraper.Library.Actors;

public class OrchestrationActor : ReceiveActor, IWithTimers
{
    private readonly Dictionary<IActorRef, bool> _childrenActorsCompleted;
    private readonly Dictionary<Guid, Type> _dispensaryToScrapeActorMapping;
    private readonly ILoggingAdapter _logger;
    private readonly TimeOnly _scheduledScrapeTime;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrchestrationActor(IServiceScopeFactory scopeFactory,
        IOptions<ScrapingScheduleConfiguration> scrapingSchedule,
        IOptions<DispensaryConfiguration> dispensaryConfigurationOptions)
    {
        _scopeFactory = scopeFactory;
        _scheduledScrapeTime = scrapingSchedule.Value.ScrapeAt;
        _childrenActorsCompleted = new Dictionary<IActorRef, bool>();
        _logger = Context.GetLogger();

        var dispensaryConfig = dispensaryConfigurationOptions.Value;
        
        _dispensaryToScrapeActorMapping = new Dictionary<Guid, Type>
        {
            { dispensaryConfig.Dispensaries.Single(p => p.Name == "QuitMed").Id, typeof(QuitmedScraperActor) }
        };

        Become(Idle);
        Self.Tell(new StartScrape());
    }

    public ITimerScheduler Timers { get; set; } = null!;

    private void Idle()
    {
        Receive<StartScrape>(_ =>
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();

            StartScrapers(dbContext);
        });
    }

    private void StartScrapers(QuitmedScraperDatabaseContext dbContext)
    {
        Become(Processing);

        List<Dispensary> dispensaries = GetDispensaries(dbContext);
        List<ExecutionLog> previousExecutionLogs = GetLastExecutionForDispensaries(dbContext);

        foreach (Dispensary dispensary in dispensaries)
        {
            ExecutionLog? previousExecution = previousExecutionLogs
                .SingleOrDefault(e => e?.Dispensary.Id == dispensary.Id);

            if (previousExecution == null || previousExecution.EndTimeUtc.Date < DateTime.UtcNow.Date)
            {
                _logger.Info("Starting scraper actor for dispensary {0}", dispensary.Name);
                StartScrapeForDispensary(dispensary);
            }
            else
            {
                _logger.Info("Skipping dispensary {0} as it has already been scraped today",
                    dispensary.Name);
            }
        }

        if (_childrenActorsCompleted.Count == 0)
        {
            _logger.Warning("No scraper actors were started. Shutting down");
            CompleteProcessing();
        }
    }

    private void Processing()
    {
        Receive<Status.Success>(_ =>
        {
            if (!_childrenActorsCompleted.ContainsKey(Sender))
            {
                _logger.Warning("Received completion message from unknown actor");
                return;
            }

            _childrenActorsCompleted[Sender] = true;

            if (_childrenActorsCompleted.Values.All(p => p))
                CompleteProcessing();
        });
    }

    private void CompleteProcessing()
    {
        _logger.Info("All scraper actors have completed.");

        Become(Idle);
        Timers.StartSingleTimer("start-scrape", new StartScrape(), GetTimeToNextScrape());
        _logger.Info("Scheduled next scrape for {0}", DateTime.Now + GetTimeToNextScrape());
    }

    private TimeSpan GetTimeToNextScrape()
    {
        DateTime now = DateTime.Now;
        DateTime nextScrape = new(now.Year, now.Month, now.Day, _scheduledScrapeTime.Hour, _scheduledScrapeTime.Minute, _scheduledScrapeTime.Second);

        if (nextScrape < now) 
            nextScrape = nextScrape.AddDays(1);

        return nextScrape - now;
    }

    private void StartScrapeForDispensary(Dispensary dispensary)
    {
        if (!_dispensaryToScrapeActorMapping.TryGetValue(dispensary.Id, out Type? scraperActorType))
        {
            _logger.Warning("No scraper actor type found for dispensary {0}", dispensary.Name);
            return;
        }

        StartScraperActorForDispensary(dispensary, scraperActorType);
    }

    private void StartScraperActorForDispensary(Dispensary dispensary, Type scraperActorType)
    {
        IActorRef? actorRef = Context.ActorOf(DependencyResolver.For(Context.System).Props(scraperActorType),
            $"{dispensary.Name}-scraper-actor");

        _childrenActorsCompleted.Add(actorRef, false);

        actorRef.Tell(new BeginScraping(dispensary));
    }

    private static List<ExecutionLog> GetLastExecutionForDispensaries(QuitmedScraperDatabaseContext dbContext)
    {
        IQueryable<IGrouping<Guid, ExecutionLog>> executionLogs = dbContext.ExecutionLogs.GroupBy(p => p.Dispensary.Id);

        List<ExecutionLog> result = new();

        foreach (IGrouping<Guid, ExecutionLog> group in executionLogs)
        {
            ExecutionLog? max = group.MaxBy(e => e.EndTimeUtc);

            if (max != null) result.Add(max);
        }

        return result;
    }

    private List<Dispensary> GetDispensaries(QuitmedScraperDatabaseContext dbContext)
    {
        return dbContext.Dispensaries.ToList();
    }

    private record StartScrape;
}