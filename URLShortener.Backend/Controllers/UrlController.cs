using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using UrlShortener.Backend.Models;
using System;

namespace UrlShortener.Backend.Controllers
{
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST /shorten
        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] UrlRequest urlRequest)
        {
            // Check if the URL already exists in the database
            var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.OriginalUrl == urlRequest.OriginalUrl);
            if (existingUrl != null)
            {
                return Ok(new { shortenedUrl = $"https://localhost:7162/{existingUrl.ShortCode}" });
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
                ShortCode = shortCode
            };

            // Save to the database
            _context.ShortUrls.Add(newUrl);
            await _context.SaveChangesAsync();

            return Ok(new { shortenedUrl = $"https://localhost:7162/{newUrl.ShortCode}" });
        }

        // GET /{shortCode}
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
            if (url == null)
            {
                return NotFound(new { message = "Short URL not found." });
            }

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
    }

    public class UrlRequest
    {
        public string OriginalUrl { get; set; }
        public string CustomAlias { get; set; }
    }
}
