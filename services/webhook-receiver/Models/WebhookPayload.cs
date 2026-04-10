namespace webhook_receiver.Models
{
    public class WebhookPayload
    {
        public string ScanId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime UpdatedAt{ get; set; } = DateTime.UtcNow;
    }
}