using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using UrlShortener.Backend.Models;

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
            var existingUrl = await _context.ShortUrls.FirstOrDefaultAsync(u => u.OriginalUrl == urlRequest.OriginalUrl);
            if (existingUrl != null)
            {
                return Ok(new { shortenedUrl = $"https://localhost:7162/{existingUrl.ShortCode}" });
            }

            var newUrl = new ShortUrl
            {
                OriginalUrl = urlRequest.OriginalUrl,
                ShortCode = urlRequest.CustomAlias
            };

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
    }

    public class UrlRequest
    {
        public string OriginalUrl { get; set; }
        public string CustomAlias { get; set; }
    }
}
