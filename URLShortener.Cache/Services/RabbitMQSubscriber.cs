using UrlShortener.Messaging.Services;
using UrlShortener.Messaging.Models;


public class RabbitMQSubscriber : IHostedService
{
    private readonly IRabbitMQService _rabbitMQ;
    private readonly IRedisService _redis;
    private readonly ILogger<RabbitMQSubscriber> _logger;

    public RabbitMQSubscriber(
        IRabbitMQService rabbitMQ,
        IRedisService redis,
        ILogger<RabbitMQSubscriber> logger)
    {
        _rabbitMQ = rabbitMQ;
        _redis = redis;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMQ.Subscribe<UrlCreatedMessage>(
            RabbitMQConstants.UrlExchange,
            RabbitMQConstants.UrlCreatedQueue,
            RabbitMQConstants.UrlCreatedRoutingKey,
            HandleUrlCreated);

        _rabbitMQ.Subscribe<UrlVisitedMessage>(
            RabbitMQConstants.UrlExchange,
            RabbitMQConstants.UrlVisitedQueue,
            RabbitMQConstants.UrlVisitedRoutingKey,
            HandleUrlVisited);

        return Task.CompletedTask;
    }

    private async void HandleUrlCreated(UrlCreatedMessage message)
    {
        try
        {
            await _redis.SetUrlAsync(message.ShortCode, message.OriginalUrl, TimeSpan.FromDays(7));
            _logger.LogInformation($"Cached URL for short code: {message.ShortCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling URL created message for {message.ShortCode}");
        }
    }

    private async void HandleUrlVisited(UrlVisitedMessage message)
    {
        try
        {
            await _redis.IncrementVisitCountAsync(message.ShortCode);
            _logger.LogInformation($"Incremented visit count for: {message.ShortCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling URL visited message for {message.ShortCode}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}