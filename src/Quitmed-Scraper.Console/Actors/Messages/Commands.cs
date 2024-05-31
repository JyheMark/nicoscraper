using Quitmed_scraper.Database.Models;

namespace Quitmed_Scraper.Console.Actors.Messages;

internal record PersistProducts(IEnumerable<Product> Products, DateTime StartTime, DateTime EndTime);
internal record BeginScraping(Dispensary Dispensary);