using scan_api.Models;
using System.Text.RegularExpressions;

namespace scan_api.Services
{
    public class ScanService
    {
        private readonly List<Scan> _scans = new();

        public Scan Create(CreateScanRequest request)
        {
            var scan = new Scan
            {
                DocumentId = request.DocumentId,
                Text = request.Text,
                CallbackUrl = request.CallbackUrl,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _scans.Add(scan);
            return scan;
        }

        public List<Scan> GetAll()
        {
            return _scans;
        }

        public Scan? GetById(string id)
        {
            return _scans.Find(x => x.Id == id);
        }

        public bool ContainsWord(string id, string word)
        {
            var scan = _scans.Find(x => x.Id == id);
            if (scan == null) {
                return false;
            }
            var safeWord = Regex.Escape(word);
            return Regex.IsMatch(scan.Text, $@"\b{safeWord}\b", RegexOptions.IgnoreCase);
        }
    }
}