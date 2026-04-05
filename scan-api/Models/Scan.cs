namespace scan_api.Models
{
    public class Scan
    {
        public string Id { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }
}