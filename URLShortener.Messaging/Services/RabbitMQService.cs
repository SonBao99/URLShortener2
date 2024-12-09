using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace UrlShortener.Messaging.Services;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService(string connectionString)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void PublishMessage<T>(string exchange, string routingKey, T message)
    {
        try
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
            
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to publish message to RabbitMQ: {ex.Message}", ex);
        }
    }

    public void Subscribe<T>(string exchange, string queue, string routingKey, Action<T> handler)
    {
        _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue, exchange, routingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(json);
            
            if (message != null)
            {
                handler(message);
            }
            
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: queue,
                            autoAck: false,
                            consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}