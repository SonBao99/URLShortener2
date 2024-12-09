using StackExchange.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<string?> GetUrlAsync(string shortCode)
    {
        return await _db.StringGetAsync(GetUrlKey(shortCode));
    }

    public async Task SetUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(GetUrlKey(shortCode), originalUrl, expiry);
    }

    public async Task<bool> DeleteUrlAsync(string shortCode)
    {
        return await _db.KeyDeleteAsync(GetUrlKey(shortCode));
    }

    public async Task<bool> KeyExistsAsync(string shortCode)
    {
        return await _db.KeyExistsAsync(GetUrlKey(shortCode));
    }

    public async Task<long> IncrementVisitCountAsync(string shortCode)
    {
        return await _db.StringIncrementAsync(GetVisitCountKey(shortCode));
    }

    public async Task<long?> GetVisitCountAsync(string shortCode)
    {
        var value = await _db.StringGetAsync(GetVisitCountKey(shortCode));
        return value.TryParse(out long count) ? count : null;
    }

    private static string GetUrlKey(string shortCode) => $"url:{shortCode}";
    private static string GetVisitCountKey(string shortCode) => $"visits:{shortCode}";
}