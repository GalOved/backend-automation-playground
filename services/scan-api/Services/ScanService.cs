using scan_api.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace scan_api.Services
{
    public class ScanService
    {
        private readonly ConcurrentDictionary<string, Scan> _scans = new();

        /// <summary>
        /// Generates a unique 5-character scan ID.
        /// </summary>
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
        /// <summary>
        /// Returns true if a scan with the given documentId already exists.
        /// </summary>
        public bool DocumentIdExists(string documentId)
        {
            return _scans.Values.Any(s => s.DocumentId == documentId);
        }

        /// <summary>
        /// Creates a new scan and adds it to the dictionary.
        /// </summary>
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

        /// <summary>
        /// Returns a list of all scans.
        /// </summary>
        public List<Scan> GetAll()
        {
            return _scans.Values.ToList();
        }

        /// <summary>
        /// Retrieves a scan by its ID.
        /// </summary>
        public Scan? GetById(string id)
        {
            _scans.TryGetValue(id, out var scan);
            return scan;
        }

        /// <summary>
        /// Checks if the scan text contains the given word (case-insensitive, word boundary).
        /// </summary>
        public bool ContainsWordInText(string id, string word)
        {
            var scan = GetById(id);
            if (scan == null) {
                return false;
            }
            var safeWord = Regex.Escape(word);
            return Regex.IsMatch(scan.Text, $@"\b{safeWord}\b", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Updates the status of a scan by ID.
        /// </summary>
        public bool UpdateStatus(string id, string status, string? errorMessage = null, string? contentHash = null)
        {
            var scan = GetById(id);
            if (scan == null)
            {
                return false;
            }

            scan.Status = status;
            scan.UpdatedAt = DateTime.UtcNow;
            scan.ErrorMessage = status == "FAILED" ? errorMessage : null;
            if (contentHash != null)
                scan.ContentHash = contentHash;
            return true;
        }

        /// <summary>
        /// Checks if the DocumentId contains the given word (case-insensitive, word boundary).
        /// </summary>
        public bool ContainsWordInDocumentId(string id, string word)
        {
            var scan = GetById(id);
            if (scan == null) {
                return false;
            }
            return scan.DocumentId.StartsWith(word, StringComparison.OrdinalIgnoreCase);       
        }
    }
}