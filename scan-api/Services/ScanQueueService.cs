using System.Collections.Concurrent;
using scan_api.Models;

namespace scan_api.Services
{
    public class ScanQueueService
    {
        private readonly ConcurrentQueue<ScanMessage> _queue = new();

        public void Enqueue(ScanMessage message)
        {
            _queue.Enqueue(message);
        }

        public bool TryDequeue(out ScanMessage? message)
        {
            return _queue.TryDequeue(out message);
        }

        public bool HasItems()
        {
            return !_queue.IsEmpty;
        }
    }
}