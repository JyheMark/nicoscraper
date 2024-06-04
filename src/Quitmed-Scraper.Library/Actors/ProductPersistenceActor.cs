using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quitmed_scraper.Database;
using Quitmed_scraper.Database.Models;
using Quitmed_Scraper.Library.Actors.Messages;

namespace Quitmed_Scraper.Library.Actors;

public class ProductPersistenceActor : ReceiveActor
{
    private readonly ILoggingAdapter _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<ProductEventBase> _productEvents;

    public ProductPersistenceActor(IServiceScopeFactory scopeFactory)
    {
        _logger = Context.GetLogger();
        _scopeFactory = scopeFactory;
        _productEvents = new List<ProductEventBase>();

        ReceiveAsync<PersistProducts>(HandlePersistProducts);
    }

    private async Task HandlePersistProducts(PersistProducts msg)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();
            dbContext.AttachRange(msg.Products.Select(p => p.Dispensary));

            List<Product> existingProducts = await dbContext.Products.Include(p => p.PriceHistory).ToListAsync();
            
            List<Product> newProducts = ProcessIncomingProducts(msg, existingProducts);
            ProcessArchivedProducts(msg, existingProducts);

            if (newProducts.Any())
                await dbContext.Products.AddRangeAsync(newProducts);
            
            await dbContext.ExecutionLogs.AddRangeAsync(GenerateExecutionLogs(msg));
            await dbContext.SaveChangesAsync();

            if (_productEvents.Any())
            {
                Context.ActorOf(DependencyResolver.For(Context.System).Props<ProductEventHandlerActor>(_productEvents.ToList()), "product-event-handler-actor");
                _productEvents.Clear();   
            }

            _logger.Info($"Persisted {msg.Products.Count()} products to database");
            Sender.Tell(new Status.Success(null), Self);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to persist products to database");
            Sender.Tell(new Status.Failure(null), Self);
        }
        finally
        {
            _logger.Info("Stopping Actor");
            Context.Stop(Self);
        }
    }

    private void ProcessArchivedProducts(PersistProducts msg, List<Product> existingProducts)
    {
        var archivedProducts = existingProducts.Where(ep => !msg.Products.Select(p => p.Key).Contains(ep.Key));
        foreach (Product product in archivedProducts)
        {
            product.IsArchived = true;
            product.InStock = false;
            _productEvents.Add(new ProductRemovedEvent
            {
                Dispensary = product.Dispensary,
                Product = product
            });
        }
    }

    private List<Product> ProcessIncomingProducts(PersistProducts msg, List<Product> existingProducts)
    {
        var newProducts = new List<Product>();

        foreach (Product product in msg.Products)
        {
            Product? existingProduct = existingProducts.SingleOrDefault(p => p.Key == product.Key);

            var priceHistoryRecord = new HistoricalPricing
            {
                Price = product.Price,
                Product = product,
                Timestamp = msg.EndTime
            };

            if (existingProduct != null)
            {
                CompareProductForEvents(product, existingProduct);
                
                existingProduct.Dispensary = product.Dispensary;
                existingProduct.Name = product.Name;
                existingProduct.Vendor = product.Vendor;
                existingProduct.Price = product.Price;
                existingProduct.InStock = product.InStock;
                existingProduct.IsArchived = false;

                if (GetLatestPriceForProduct(existingProduct) == null || GetLatestPriceForProduct(existingProduct)!.Price != product.Price)
                    existingProduct.PriceHistory.Add(priceHistoryRecord);
            }
            else
            {
                product.PriceHistory = [priceHistoryRecord];
                newProducts.Add(product);
                
                _productEvents.Add(new ProductAddedEvent
                {
                    Dispensary = product.Dispensary,
                    Product = product
                });
            }
        }
        
        return newProducts;
    }

    private void CompareProductForEvents(Product incomingProduct, Product existingProduct)
    {
        CheckForPriceChangeEvent(incomingProduct, existingProduct);
        CheckForStockStatusChangeEvent(incomingProduct, existingProduct);
    }

    private void CheckForStockStatusChangeEvent(Product incomingProduct, Product existingProduct)
    {
        if (existingProduct.InStock != incomingProduct.InStock)
            _productEvents.Add(new ProductStockStatusUpdatedEvent()
            {
                Dispensary = incomingProduct.Dispensary,
                Product = incomingProduct,
                InStock = incomingProduct.InStock
            });
    }

    private void CheckForPriceChangeEvent(Product incomingProduct, Product existingProduct)
    {
        var previousPrice = GetLatestPriceForProduct(existingProduct);

        if (previousPrice == null || previousPrice.Price != incomingProduct.Price)
            _productEvents.Add(new ProductPriceChangeEvent
            {
                Dispensary = incomingProduct.Dispensary,
                Product = incomingProduct,
                PreviousPrice = previousPrice?.Price ?? 0,
                NewPrice = incomingProduct.Price
            });
    }

    private static HistoricalPricing? GetLatestPriceForProduct(Product product)
    {
        return product.PriceHistory.MaxBy(p => p.Timestamp);
    }

    private static IEnumerable<ExecutionLog> GenerateExecutionLogs(PersistProducts msg)
    {
        return msg.Products.DistinctBy(p => p.Dispensary.Id).Select(p => new ExecutionLog
        {
            StartTimeUtc = msg.StartTime,
            EndTimeUtc = msg.EndTime,
            Dispensary = p.Dispensary
        });
    }
}