using Akka.Actor;
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

    public ProductPersistenceActor(IServiceScopeFactory scopeFactory)
    {
        _logger = Context.GetLogger();
        _scopeFactory = scopeFactory;

        ReceiveAsync<PersistProducts>(HandlePersistProducts);
    }

    private async Task HandlePersistProducts(PersistProducts msg)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<QuitmedScraperDatabaseContext>();
            dbContext.AttachRange(msg.Products.Select(p => p.Dispensary));

            List<Product> existingProducts = await GetBatchProductsById(msg.Products, dbContext);
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
                    existingProduct.Dispensary = product.Dispensary;
                    existingProduct.Name = product.Name;
                    existingProduct.Vendor = product.Vendor;
                    existingProduct.Price = product.Price;
                    existingProduct.InStock = product.InStock;

                    if (existingProduct.PriceHistory.MaxBy(p => p.Timestamp)?.Price != product.Price)
                        existingProduct.PriceHistory.Add(priceHistoryRecord);
                }
                else
                {
                    product.PriceHistory = [priceHistoryRecord];
                    newProducts.Add(product);
                }
            }

            if (newProducts.Any())
                await dbContext.Products.AddRangeAsync(newProducts);
            
            await dbContext.ExecutionLogs.AddRangeAsync(GenerateExecutionLogs(msg));
            await dbContext.SaveChangesAsync();

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

    private static IEnumerable<ExecutionLog> GenerateExecutionLogs(PersistProducts msg)
    {
        return msg.Products.DistinctBy(p => p.Dispensary.Id).Select(p => new ExecutionLog
        {
            StartTimeUtc = msg.StartTime,
            EndTimeUtc = msg.EndTime,
            Dispensary = p.Dispensary
        });
    }

    private static async Task<List<Product>> GetBatchProductsById(IEnumerable<Product> products, QuitmedScraperDatabaseContext dbContext)
    {
        return await dbContext.Products.Include(p => p.PriceHistory).Where(p => products.Select(q => q.Key).Contains(p.Key)).ToListAsync();
    }
}