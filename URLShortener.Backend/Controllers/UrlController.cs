using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using UrlShortener.Backend.Models;
using System.Security.Claims;
using UrlShortener.Messaging.Services;
using UrlShortener.Messaging.Models;
using Microsoft.AspNetCore.Authorization;
namespace UrlShortener.Backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UrlController> _logger;
        private readonly IRabbitMQService _rabbitMQ;

        public UrlController(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<UrlController> logger, IRabbitMQService rabbitMQ)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _rabbitMQ = rabbitMQ;
        }

        // POST /shorten
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlRequest urlRequest)
        {
            var baseUrl = _configuration["UrlSettings:BaseUrl"];
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Validate expiration days
            if (urlRequest.ExpirationDays.HasValue && (urlRequest.ExpirationDays.Value <= 0 || urlRequest.ExpirationDays.Value > 365))
            {
                return BadRequest("Expiration days must be between 1 and 365");
            }

            // Check if the URL already exists in the database
            var existingUrl = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.OriginalUrl == urlRequest.OriginalUrl && u.ExpirationDate > DateTime.UtcNow);
            
            if (existingUrl != null)
            {
                return Ok(new { shortenedUrl = $"{baseUrl}{existingUrl.ShortCode}" });
            }

            // Generate a unique short code if CustomAlias is not provided
            string shortCode = urlRequest.CustomAlias;
            if (string.IsNullOrEmpty(shortCode))
            {
                shortCode = await GenerateUniqueShortCodeAsync();
            }

            // Create a new ShortUrl object
            var newUrl = new ShortUrl
            {
                OriginalUrl = urlRequest.OriginalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(urlRequest.ExpirationDays ?? 1),
                UserId = userId
            };

            // Save to the database
            _context.ShortUrls.Add(newUrl);
            await _context.SaveChangesAsync();

            // Publish message to RabbitMQ
            var message = new UrlCreatedMessage
            {
                ShortCode = newUrl.ShortCode,
                OriginalUrl = newUrl.OriginalUrl,
                UserId = newUrl.UserId,
                CreatedAt = newUrl.CreatedAt
            };

            try
            {
                _rabbitMQ.PublishMessage(
                    RabbitMQConstants.UrlExchange,
                    RabbitMQConstants.UrlCreatedRoutingKey,
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to RabbitMQ");
                // Continue execution, don't fail the request
            }

            return Ok(new { shortenedUrl = $"{baseUrl}{newUrl.ShortCode}" });
        }

        // GET /{shortCode}
        [Route("{shortCode}")]
        [HttpGet]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var url = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode && u.ExpirationDate > DateTime.UtcNow);
            
            if (url == null)
                return NotFound("URL not found or has expired");

            // Increment database count
            url.UsageCount++;
            await _context.SaveChangesAsync();

            // Cache the URL and increment Redis count
            using var httpClient = _httpClientFactory.CreateClient();
            try 
            {
                await httpClient.PostAsJsonAsync("https://localhost:7000/api/cache", new
                {
                    ShortCode = url.ShortCode,
                    OriginalUrl = url.OriginalUrl
                });

                await httpClient.PostAsync($"https://localhost:7000/api/cache/{shortCode}/increment", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update cache for {ShortCode}", shortCode);
            }

            // Publish visit message
            var message = new UrlVisitedMessage
            {
                ShortCode = shortCode,
                VisitedAt = DateTime.UtcNow,
                UserId = User.Identity?.IsAuthenticated == true ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null
            };

            _rabbitMQ.PublishMessage(
                RabbitMQConstants.UrlExchange,
                RabbitMQConstants.UrlVisitedRoutingKey,
                message);

            return Redirect(url.OriginalUrl);
        }

        // Helper method to generate a unique short code
        private async Task<string> GenerateUniqueShortCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            string shortCode;

            do
            {
                shortCode = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode));

            return shortCode;
        }

        [HttpGet("urls")]
        public async Task<IActionResult> GetUserUrls()
        {
            var baseUrl = _configuration["UrlSettings:BaseUrl"];
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var urls = await _context.ShortUrls
                .Where(u => u.UserId == userId && u.ExpirationDate > DateTime.UtcNow)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    originalUrl = u.OriginalUrl,
                    shortenedUrl = $"{baseUrl}{u.ShortCode}",
                    createdAt = u.CreatedAt,
                    expirationDate = u.ExpirationDate,
                    usageCount = u.UsageCount
                })
                .ToListAsync();

            return Ok(urls);
        }

        // Add this endpoint for clearing URLs
        [HttpDelete("urls/clear")]
        public async Task<IActionResult> ClearUserUrls()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var userUrls = await _context.ShortUrls
                .Where(u => u.UserId == userId)
                .ToListAsync();

            _context.ShortUrls.RemoveRange(userUrls);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete("urls/{shortCode}")]
        public async Task<IActionResult> DeleteUrl(string shortCode)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var url = await _context.ShortUrls.FirstOrDefaultAsync(u => 
                u.ShortCode == shortCode && u.UserId == userId);
            
            if (url == null)
                return NotFound();

            _context.ShortUrls.Remove(url);
            await _context.SaveChangesAsync();

            // Also remove from cache if it exists
            try 
            {
                using var httpClient = _httpClientFactory.CreateClient();
                await httpClient.DeleteAsync($"https://localhost:7000/api/cache/{shortCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove URL from cache: {ShortCode}", shortCode);
            }

            return Ok();
        }
    }

    public class UrlRequest
    {
        public required string OriginalUrl { get; set; }
        public string? CustomAlias { get; set; }
        public int? ExpirationDays { get; set; } // Optional, defaults to 1 if not provided
    }
}
