using System.Text.Json;
using System.Text.RegularExpressions;
using Quitmed_Scraper.Console.QuitMed;

// var httpClient = new HttpClient();
// httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
// var response = await httpClient.GetAsync("https://quitmed.com.au/collections/all");
// var responseBody = await response.Content.ReadAsStringAsync();

var responseBody = File.ReadAllText(@"D:\Jyhe\Downloads\sample-data.html");

var metaData = Regex.Match(responseBody, @"var meta = (.*?);", RegexOptions.Singleline);

var productMetadata = JsonSerializer.Deserialize<ProductMetadataWrapper>(metaData.Groups[1].Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

foreach (var product in productMetadata.Products)
{
    var variant = product.Variants.First();

    var htmlCapture = Regex.Match(responseBody, $"<div id=\"product-{product.Id}\"(.*?)>", RegexOptions.Singleline);
    if (htmlCapture.Success && htmlCapture.Groups[1].Value.Contains("sold-out"))
        Console.WriteLine($"{variant.Name} is SOLD OUT");
}