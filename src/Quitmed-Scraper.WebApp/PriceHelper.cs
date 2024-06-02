namespace Quitmed_Scraper.WebApp;

internal static class PriceHelper
{
    public static string FormatAsPrice(int price)
    {
        return (price / 100.0m).ToString("C");
    }
    
    public static double FormatAsPriceDouble(int price)
    {
        return (double)price / 100.0;
    }
}