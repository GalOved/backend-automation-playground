using Microsoft.Extensions.Hosting;
using scan_api.Services;

namespace scan_api.BackgroundServices
{
    public class ScanProcessingWorker : BackgroundService
    {
        private readonly ScanQueueService _queue;
        private readonly ScanService _scanService;

        public ScanProcessingWorker(ScanQueueService queue, ScanService scanService)
        {
            _queue = queue;
            _scanService = scanService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("ScanProcessingWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var message) && message != null)
                {
                    Console.WriteLine($"Started processing scan {message.ScanId}");

                    _scanService.UpdateStatus(message.ScanId, "PROCESSING");

                    await Task.Delay(10_000, stoppingToken);

                    _scanService.UpdateStatus(message.ScanId, "COMPLETED");

                    Console.WriteLine($"Completed processing scan {message.ScanId}");
                }
                else
                {
                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}