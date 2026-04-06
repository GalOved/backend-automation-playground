using Microsoft.Extensions.Hosting;
using scan_api.Models;
using scan_api.Services;

namespace scan_api.BackgroundServices
{
    public class ScanProcessingWorker : BackgroundService
    {
        private readonly ScanQueueService _queue;
        private readonly ScanService _scanService;
        private const string FAIL = "fail";
        private const string BAD = "bad-";

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
                ScanMessage? message = null;
                try
                {
                    if (_queue.TryDequeue(out message) && message != null)
                    {
                        Console.WriteLine($"Started processing scan {message.ScanId}");
                        _scanService.UpdateStatus(message.ScanId, "PROCESSING");
                        var containsFail = _scanService.ContainsWordInText(message.ScanId, FAIL);
                        var containsBadInDocumentId = _scanService.ContainsWordInDocumentId(message.ScanId, BAD);
                        await Task.Delay(5_000, stoppingToken);
                        if (containsFail || containsBadInDocumentId)
                        {
                            _scanService.UpdateStatus(message.ScanId, "FAILED", "Scan processing failed.");
                            Console.WriteLine($"Failed processing scan {message.ScanId}");
                            continue;
                        }
                        _scanService.UpdateStatus(message.ScanId, "COMPLETED");
                        Console.WriteLine($"Completed processing scan {message.ScanId}");
                    }
                    else
                    {
                        await Task.Delay(500, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing scan: {ex.Message}\n{ex.StackTrace}");
                    if (message != null)
                    {
                        _scanService.UpdateStatus(message.ScanId, "FAILED", ex.Message);
                    }
                }
            }
        }
    }
}