using scan_api.Models;
using System.Text.RegularExpressions;

namespace scan_api.Services
{
    public class ScanService
    {
        private readonly Dictionary<string, Scan> _scans = new();

        private string GenerateUniqueId()
        {
            string id;

            do
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 5);
            }
            while (_scans.ContainsKey(id));

            return id;
        }
        public Scan Create(CreateScanRequest request)
        {
            var scan = new Scan
            {
                Id = GenerateUniqueId(),
                DocumentId = request.DocumentId,
                Text = request.Text,
                CallbackUrl = request.CallbackUrl,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _scans[scan.Id] = scan;
            return scan;
        }

        public List<Scan> GetAll()
        {
            return _scans.Values.ToList();
        }

        public Scan? GetById(string id)
        {
            _scans.TryGetValue(id, out var scan);
            return scan;
        }

        public bool ContainsWord(string id, string word)
        {
            var scan = GetById(id);
            if (scan == null) {
                return false;
            }
            var safeWord = Regex.Escape(word);
            return Regex.IsMatch(scan.Text, $@"\b{safeWord}\b", RegexOptions.IgnoreCase);
        }
    }
}