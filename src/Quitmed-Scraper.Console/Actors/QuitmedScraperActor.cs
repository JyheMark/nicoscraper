using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Quitmed_Scraper.Console.Actors.Messages;
using Quitmed_Scraper.Console.QuitMed;
using Quitmed_scraper.Database.Models;

namespace Quitmed_Scraper.Console.Actors;

internal class QuitmedScraperActor : ReceiveActor
{
    private readonly ILoggingAdapter _logger;
    private readonly HttpClient _httpClient;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private List<Product>? _products;
    private IActorRef? _sender;

    public QuitmedScraperActor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _logger = Context.GetLogger();
        _logger.Info("Starting");
        _products = new List<Product>();
        ReceiveAsync<BeginScraping>(HandleBeginScraping);   
    }

    private async Task HandleBeginScraping(BeginScraping msg)
    {
        _sender = Sender;
        Become(Processing);
     
        _logger.Info("Beginning scraping of QuitMed stock page");

        _startTime = DateTime.UtcNow;
        var response = await _httpClient.GetAsync(msg.Dispensary.ScrapeUrl);

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"Failed to fetch QuitMed stock page. Server Response: {response.StatusCode}");
            throw new Exception("Failed to fetch QuitMed stock page");
        }
        
        var responseBody = await response.Content.ReadAsStringAsync();

        var metaDataString = FindMetadataString(responseBody);
        var productMetadata = JsonSerializer.Deserialize<ProductMetadataWrapper>(metaDataString, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        if (productMetadata == null)
        {
            _logger.Error("Failed to deserialize product metadata");
            throw new Exception("Failed to deserialize product metadata");
        }

        _products = productMetadata.Products.Select(p => new Product
        {
            Key = GenerateKey(p),
            Name = p.Variants.First().Name,
            Vendor = p.Vendor,
            Price = p.Variants.First().Price,
            InStock = !IsSoldOut(responseBody, p.Id),
            Dispensary = msg.Dispensary
        }).ToList();

        _endTime = DateTime.UtcNow;
        _logger.Info($"Found {_products.Count} products on QuitMed stock page");

        SendProductsToPersistenceActor();
    }

    private void SendProductsToPersistenceActor()
    {
        ArgumentNullException.ThrowIfNull(_startTime);
        ArgumentNullException.ThrowIfNull(_endTime);
        ArgumentNullException.ThrowIfNull(_products);
        
        Context
            .ActorOf(DependencyResolver.For(Context.System).Props<ProductPersistenceActor>(), "persistence-actor")
            .Tell(new PersistProducts(_products, _startTime.Value, _endTime.Value));
    }

    private void Processing()
    {
        Receive<Status.Success>(msg =>
        {
            _logger.Info("Persistence actor reported success");
            _products = null;
            _startTime = null;
            _endTime = null;
            _sender?.Tell(msg, Self);
            Context.Stop(Self);
        });
        Receive<Status.Failure>(_ =>
        {
            _logger.Warning("Persistence actor reported failure. Retrying");
            SendProductsToPersistenceActor();
        });
    }

    private static string GenerateKey(ProductMetadata productMetadata)
    {
        var productId = productMetadata.Id;
        var variantId = productMetadata.Variants.First().Id;
        var key = $"{productId}:{variantId}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
    }

    private static string FindMetadataString(string html)
    {
        return Regex.Match(html, @"var meta = (.*?);", RegexOptions.Singleline).Groups[1].Value;
    }

    private static bool IsSoldOut(string html, long productId)
    {
        Match htmlCapture = Regex.Match(html, $"<div id=\"product-{productId}\"(.*?)>",
            RegexOptions.Singleline);
        
        if (!htmlCapture.Success)
            throw new Exception("Product not found in HTML");

        return htmlCapture.Groups[1].Value.Contains("sold-out");
    }
}