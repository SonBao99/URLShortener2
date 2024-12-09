namespace UrlShortener.Messaging.Models;

public class UrlCreatedMessage
{
    public required string ShortCode { get; set; }
    public required string OriginalUrl { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
