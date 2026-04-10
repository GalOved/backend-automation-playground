using System.ComponentModel.DataAnnotations;

namespace scan_api.Models
{
    public class CreateScanRequest
    {
        [Required]
        public string DocumentId { get; set; } = string.Empty;

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        [Url]
        public string CallbackUrl { get; set; } = string.Empty;
    }
}