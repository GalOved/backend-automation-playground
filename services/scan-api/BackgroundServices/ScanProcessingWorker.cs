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
        private readonly ILogger<ScanProcessingWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private IConnection? _connection;
        private IChannel? _channel;
        private string _queueName = "scan-queue";

        public ScanProcessingWorker(ScanService scanService, IConfiguration configuration, ILogger<ScanProcessingWorker> logger, IHttpClientFactory httpClientFactory)
        {
            _scanService = scanService;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var hostName = _configuration["RabbitMq:HostName"] ?? "localhost";
            var userName = _configuration["RabbitMq:UserName"] ?? "guest";
            var password = _configuration["RabbitMq:Password"] ?? "guest";
            _queueName = _configuration["RabbitMq:QueueName"] ?? "scan-queue";

            _logger.LogInformation("Connecting to RabbitMQ at {HostName}, queue: {QueueName}", hostName, _queueName);

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

            _logger.LogInformation("RabbitMQ consumer ready on queue: {QueueName}", _queueName);

            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ ScanProcessingWorker started.");

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
                        _logger.LogWarning("Received an empty or undeserializable message from queue.");
                        return;
                    }

                    _logger.LogInformation("Received scan message for ScanId: {ScanId}", message.ScanId);
                    _scanService.UpdateStatus(message.ScanId, "PROCESSING");

                    var containsFail = _scanService.ContainsWordInText(message.ScanId, "fail");
                    var hasBadDocumentId = _scanService.ContainsWordInDocumentId(message.ScanId, "bad-");

                    await Task.Delay(10_000, stoppingToken);

                    if (containsFail || hasBadDocumentId)
                    {
                        _scanService.UpdateStatus(message.ScanId, "FAILED", "Scan processing failed.");
                        _logger.LogWarning("Scan {ScanId} marked as FAILED", message.ScanId);
                        await SendWebhookAsync(message.ScanId, "FAILED", "Scan processing failed.", stoppingToken);
                        return;
                    }

                    _scanService.UpdateStatus(message.ScanId, "COMPLETED");
                    _logger.LogInformation("Scan {ScanId} completed successfully", message.ScanId);
                    await SendWebhookAsync(message.ScanId, "COMPLETED", null, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scan message");

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

        private async Task SendWebhookAsync(string scanId, string status, string? errorMessage, CancellationToken cancellationToken)
        {
            var scan = _scanService.GetById(scanId);
            if (scan == null || string.IsNullOrEmpty(scan.CallbackUrl))
            {
                _logger.LogWarning("Scan {ScanId} has no callbackUrl, skipping webhook", scanId);
                return;
            }

            var payload = new
            {
                scanId,
                status,
                errorMessage,
                updatedAt = DateTime.UtcNow
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(scan.CallbackUrl, content, cancellationToken);
                _logger.LogInformation("Webhook sent for {ScanId} → {StatusCode}", scanId, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for {ScanId} to {CallbackUrl}", scanId, scan.CallbackUrl);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}