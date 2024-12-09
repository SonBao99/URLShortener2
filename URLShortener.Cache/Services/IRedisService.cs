public interface IRedisService
{
    Task<string?> GetUrlAsync(string shortCode);
    Task SetUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null);
    Task<bool> DeleteUrlAsync(string shortCode);
    Task<bool> KeyExistsAsync(string shortCode);
    Task<long> IncrementVisitCountAsync(string shortCode);
    Task<long?> GetVisitCountAsync(string shortCode);
}