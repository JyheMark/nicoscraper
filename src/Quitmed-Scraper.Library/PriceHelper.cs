using System.Globalization;

namespace Quitmed_Scraper.Library;

public static class PriceHelper
{
    public static string FormatAsPrice(int price)
    {
        return (price / 100.0m).ToString("C", new CultureInfo("en-AU"));
    }
    
    public static double FormatAsPriceDouble(int price)
    {
        return (double)price / 100.0;
    }
}