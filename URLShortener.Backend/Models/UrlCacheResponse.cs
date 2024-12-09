namespace UrlShortener.Backend.Models;

public class UrlCacheResponse
{
    public required string Url { get; set; }
    public long? Visits { get; set; }
} 