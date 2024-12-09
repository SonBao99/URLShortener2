namespace UrlShortener.Messaging.Models;

public class UrlVisitedMessage
{
    public required string ShortCode { get; set; }
    public DateTime VisitedAt { get; set; }
    public string? UserId { get; set; }
}

