namespace Quitmed_Scraper.Library.QuitMed;

public record ProductMetadataWrapper
{
    public IEnumerable<ProductMetadata> Products { get; set; }
}

public record ProductMetadata
{
    public long Id { get; init; }
    public string Gid { get; init; }
    public string Vendor { get; init; }
    public string Type { get; init; }
    public IEnumerable<Variant> Variants { get; init; }

    public record Variant
    {
        public long Id { get; init; }
        public int Price { get; init; }
        public string Name { get; init; }
        public string Sku { get; init; }
    }
}