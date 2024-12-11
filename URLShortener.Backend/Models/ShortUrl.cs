namespace UrlShortener.Backend.Models;

public class ShortUrl
{
    public int Id { get; set; }
    public required string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int UsageCount { get; set; }
    public string? UserId { get; set; }
}
