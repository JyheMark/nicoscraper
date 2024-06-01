using Quitmed_scraper.Database.Models;

namespace Quitmed_Scraper.Library.Actors.Messages;

public record PersistProducts(IEnumerable<Product> Products, DateTime StartTime, DateTime EndTime);
public record BeginScraping(Dispensary Dispensary);