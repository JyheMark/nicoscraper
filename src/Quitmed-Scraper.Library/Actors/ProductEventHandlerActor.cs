using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.DependencyInjection;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Models;
using Quitmed_Scraper.Library.Actors.Messages;

namespace Quitmed_Scraper.Library.Actors;

internal class ProductEventHandlerActor : ReceiveActor
{
    public ProductEventHandlerActor(IServiceScopeFactory serviceScopeFactory, List<ProductEventBase> events)
    {
        var logger = Context.GetLogger();
        logger.Info($"Storing {events.Count} product events in database");

        var eventSummaries = new List<EventSummary>();
        
        foreach (var message in events)
        {
            switch (message)
            {
                case ProductAddedEvent evt:
                    eventSummaries.Add(CreateEventSummary(evt));
                    break;
                case ProductPriceChangeEvent evt:
                    eventSummaries.Add(CreateEventSummary(evt));
                    break;
                case ProductStockStatusUpdatedEvent evt:
                    eventSummaries.Add(CreateEventSummary(evt));
                    break;
                case ProductRemovedEvent evt:
                    eventSummaries.Add(CreateEventSummary(evt));
                    break;
                default:
                    throw new InvalidOperationException("Unknown product event type");
            }
        }
        
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();
        dbContext.AttachRange(eventSummaries.Select(p => p.Product));
        
        dbContext.EventSummaries.AddRange(eventSummaries);
        dbContext.SaveChanges();
        
        logger.Info("Stopping Actor");
        Context.Stop(Self);
    }

    private EventSummary CreateEventSummary(ProductRemovedEvent msg)
    {
        return CreateEventSummary(msg, $"{msg.Dispensary.Name} removed product {msg.Product.Name}");
    }

    private EventSummary CreateEventSummary(ProductStockStatusUpdatedEvent msg)
    {
        return CreateEventSummary(msg, $"{msg.Product.Name} is now {(msg.InStock ? "in stock" : "out of stock")} at {msg.Dispensary.Name}");
    }

    private EventSummary CreateEventSummary(ProductPriceChangeEvent msg)
    {
        return CreateEventSummary(msg, $"{msg.Dispensary.Name} updated price of {msg.Product.Name} from {PriceHelper.FormatAsPrice(msg.PreviousPrice)} to {PriceHelper.FormatAsPrice(msg.NewPrice)}");
    }

    private EventSummary CreateEventSummary(ProductAddedEvent msg)
    {
        return CreateEventSummary(msg, $"{msg.Dispensary.Name} added product {msg.Product.Name}");
    }

    private EventSummary CreateEventSummary(ProductEventBase msg, string message)
    {
        return new EventSummary
        {
            Dispensary = msg.Dispensary,
            Product = msg.Product,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };
    }
}