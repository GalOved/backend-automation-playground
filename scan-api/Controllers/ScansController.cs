using Microsoft.AspNetCore.Mvc;
using scan_api.Models;

namespace scan_api.Controllers
{
    [ApiController]
    [Route("scans")]
    public class ScansController : ControllerBase
    {
        private static readonly List<Scan> Scans = new();

        [HttpPost]
        public IActionResult Create([FromBody] CreateScanRequest request)
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

            Scans.Add(scan);

            return Accepted(new
            {
                id = scan.Id,
                status = scan.Status
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Scans);
        }

        [HttpGet("{Id}")]
        public IActionResult GetScans(string Id)
        {
            var scan = Scans.Find(x => x.Id == Id);
            if (scan == null)
            {
                return NotFound();
            }
            return Ok(scan);
        }
    }
}