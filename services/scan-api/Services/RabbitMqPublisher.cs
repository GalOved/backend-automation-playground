using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using scan_api.Models;

namespace scan_api.Services
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;

            var hostName = configuration["RabbitMq:HostName"] ?? "localhost";
            var userName = configuration["RabbitMq:UserName"] ?? "guest";
            var password = configuration["RabbitMq:Password"] ?? "guest";
            _queueName = configuration["RabbitMq:QueueName"] ?? "scan-queue";

            _logger.LogInformation("Connecting to RabbitMQ at {HostName}, queue: {QueueName}", hostName, _queueName);

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            ).GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ publisher ready on queue: {QueueName}", _queueName);
        }

        public void Publish(ScanMessage message)
        {
            _logger.LogInformation("Publishing scan message for ScanId: {ScanId}", message.ScanId);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body
            ).GetAwaiter().GetResult();

            _logger.LogInformation("Successfully published scan message for ScanId: {ScanId}", message.ScanId);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}