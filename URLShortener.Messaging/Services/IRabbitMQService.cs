namespace UrlShortener.Messaging.Services;

public interface IRabbitMQService
{
    void PublishMessage<T>(string exchange, string routingKey, T message);
    void Subscribe<T>(string exchange, string queue, string routingKey, Action<T> handler);
}