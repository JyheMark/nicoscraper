using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Microsoft.Extensions.DependencyInjection;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Models;
using Quitmed_Scraper.Library.Actors.Messages;

namespace Quitmed_Scraper.Library.Actors;

public class OrchestrationActor : ReceiveActor
{
    private readonly Dictionary<Guid, Type> _dispensaryToScrapeActorMapping;
    private readonly Dictionary<IActorRef, bool> _childrenActorsCompleted;
    private readonly ILoggingAdapter _logger;

    public OrchestrationActor(IServiceScopeFactory scopeFactory)
    {
        _childrenActorsCompleted = new Dictionary<IActorRef, bool>();
        _logger = Context.GetLogger();
        _dispensaryToScrapeActorMapping = new Dictionary<Guid, Type>
        {
            { Guid.Parse("c123f55e-9d6b-4dd4-9754-11cddd50ef62"), typeof(QuitmedScraperActor) }
        };

        using IServiceScope scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();

        StartScrapers(dbContext);
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
        _logger.Info("All scraper actors have completed. Shutting down");
        Context.Stop(Self);
        Context.System.Terminate();
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
        var executionLogs = dbContext.ExecutionLogs.GroupBy(p => p.Dispensary.Id);
        
        List<ExecutionLog> result = new();
        
        foreach (var group in executionLogs)
        {
            var max = group.MaxBy(e => e.EndTimeUtc);
            
            if (max != null)
            {
                result.Add(max);
            }
        }

        return result;
    }

    private List<Dispensary> GetDispensaries(QuitmedScraperDatabaseContext dbContext)
    {
        return dbContext.Dispensaries.ToList();
    }
}