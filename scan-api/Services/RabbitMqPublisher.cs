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

        public RabbitMqPublisher(IConfiguration configuration)
        {
            var hostName = configuration["RabbitMq:HostName"] ?? "localhost";
            var userName = configuration["RabbitMq:UserName"] ?? "guest";
            var password = configuration["RabbitMq:Password"] ?? "guest";
            _queueName = configuration["RabbitMq:QueueName"] ?? "scan-queue";

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
        }

        public void Publish(ScanMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body
            ).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}