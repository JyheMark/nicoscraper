using Quitmed_scraper.Database.Models;

namespace Quitmed_Scraper.Library.Actors.Messages;

internal abstract record ProductEventBase
{
    public Dispensary Dispensary { get; init; }
    public Product Product { get; init; }
}

internal record ProductAddedEvent : ProductEventBase;

internal record ProductRemovedEvent : ProductEventBase;

internal record ProductPriceChangeEvent : ProductEventBase
{
    public int PreviousPrice { get; set; }
    public int NewPrice { get; set; }
}

internal record ProductStockStatusUpdatedEvent : ProductEventBase
{
    public bool InStock { get; set; }
}