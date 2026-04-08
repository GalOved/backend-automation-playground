using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using scan_api.Models;
using scan_api.Services;

namespace scan_api.BackgroundServices
{
    public class ScanProcessingWorker : BackgroundService
    {
        private readonly ScanService _scanService;
        private readonly IConfiguration _configuration;

        private IConnection? _connection;
        private IChannel? _channel;
        private string _queueName = "scan-queue";

        public ScanProcessingWorker(ScanService scanService, IConfiguration configuration)
        {
            _scanService = scanService;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var hostName = _configuration["RabbitMq:HostName"] ?? "localhost";
            var userName = _configuration["RabbitMq:UserName"] ?? "guest";
            var password = _configuration["RabbitMq:Password"] ?? "guest";
            _queueName = _configuration["RabbitMq:QueueName"] ?? "scan-queue";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );

            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("RabbitMQ ScanProcessingWorker started.");

            var consumer = new AsyncEventingBasicConsumer(_channel!);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                ScanMessage? message = null;

                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    message = JsonSerializer.Deserialize<ScanMessage>(json);

                    if (message == null)
                    {
                        return;
                    }

                    Console.WriteLine($"Started processing scan {message.ScanId}");
                    _scanService.UpdateStatus(message.ScanId, "PROCESSING");

                    var containsFail = _scanService.ContainsWordInText(message.ScanId, "fail");
                    var hasBadDocumentId = _scanService.ContainsWordInDocumentId(message.ScanId, "bad-");

                    await Task.Delay(10_000, stoppingToken);

                    if (containsFail || hasBadDocumentId)
                    {
                        _scanService.UpdateStatus(message.ScanId, "FAILED", "Scan processing failed.");
                        Console.WriteLine($"Scan {message.ScanId} marked as FAILED");
                        return;
                    }

                    _scanService.UpdateStatus(message.ScanId, "COMPLETED");
                    Console.WriteLine($"Completed processing scan {message.ScanId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing scan: {ex.Message}");

                    if (message != null)
                    {
                        _scanService.UpdateStatus(message.ScanId, "FAILED", ex.Message);
                    }
                }
            };

            _channel!.BasicConsumeAsync(
                queue: _queueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}