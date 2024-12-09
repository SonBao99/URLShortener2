using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using UrlShortener.Backend.Models;
using System;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

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

        public UrlController(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<UrlController> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // POST /shorten
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlRequest urlRequest)
        {
            var baseUrl = _configuration["UrlSettings:BaseUrl"];
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Check if the URL already exists in the database
            var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.OriginalUrl == urlRequest.OriginalUrl);
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
                UserId = userId
            };

            // Save to the database
            _context.ShortUrls.Add(newUrl);
            await _context.SaveChangesAsync();

            return Ok(new { shortenedUrl = $"{baseUrl}{newUrl.ShortCode}" });
        }

        // GET /{shortCode}
        [Route("{shortCode}")]
        [HttpGet]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            // Try to get URL from Redis first
            try
            {
                using var cacheClient = _httpClientFactory.CreateClient();
                var cacheResponse = await cacheClient.GetAsync($"https://localhost:7168/api/cache/{shortCode}");
                if (cacheResponse.IsSuccessStatusCode)
                {
                    var cacheResult = await cacheResponse.Content.ReadFromJsonAsync<UrlCacheResponse>();
                    if (cacheResult != null)
                    {
                        return Redirect(cacheResult.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue to database lookup
                _logger.LogError(ex, "Error accessing cache for shortCode: {ShortCode}", shortCode);
            }

            // If not in cache, get from database
            var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
            if (url == null)
            {
                return NotFound(new { message = "Short URL not found." });
            }

            // Cache the URL
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync("https://localhost:7168/api/cache", new
            {
                ShortCode = url.ShortCode,
                OriginalUrl = url.OriginalUrl
            });

            // Increment usage count
            url.UsageCount++;
            await _context.SaveChangesAsync();

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
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    originalUrl = u.OriginalUrl,
                    shortenedUrl = $"{baseUrl}{u.ShortCode}",
                    createdAt = u.CreatedAt,
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
    }

    public class UrlRequest
    {
        public required string OriginalUrl { get; set; }
        public string? CustomAlias { get; set; }
    }
}
