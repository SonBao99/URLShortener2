using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(IRedisService redisService, ILogger<CacheController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> GetUrl(string shortCode)
    {
        var url = await _redisService.GetUrlAsync(shortCode);
        if (url == null)
            return NotFound();

        await _redisService.IncrementVisitCountAsync(shortCode);
        return Ok(new { url, visits = await _redisService.GetVisitCountAsync(shortCode) });
    }

    [HttpPost]
    public async Task<IActionResult> SetUrl([FromBody] UrlCacheRequest request)
    {
        await _redisService.SetUrlAsync(request.ShortCode, request.OriginalUrl, TimeSpan.FromDays(30));
        return Ok();
    }

    [HttpDelete("{shortCode}")]
    public async Task<IActionResult> DeleteUrl(string shortCode)
    {
        var deleted = await _redisService.DeleteUrlAsync(shortCode);
        return deleted ? Ok() : NotFound();
    }
}

public class UrlCacheRequest
{
    public required string ShortCode { get; set; }
    public required string OriginalUrl { get; set; }
}