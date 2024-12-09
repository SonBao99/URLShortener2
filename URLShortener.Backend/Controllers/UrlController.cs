using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using UrlShortener.Backend.Models;
using System;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace UrlShortener.Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UrlController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST /shorten
        [Route("api/shorten")]
        [HttpPost]
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
            var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
            if (url == null)
            {
                return NotFound(new { message = "Short URL not found." });
            }

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

        [Route("api/urls")]
        [HttpGet]
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
        [Route("api/urls/clear")]
        [HttpDelete]
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
