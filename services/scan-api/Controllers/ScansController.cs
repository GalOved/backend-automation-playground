using Microsoft.AspNetCore.Mvc;
using scan_api.Models;
using scan_api.Services;

namespace scan_api.Controllers
{
    [ApiController]
    [Route("scans")]
    public class ScansController : ControllerBase
    {
        private readonly ScanService _scanService;
        private readonly RabbitMqPublisher _publisher;

        public ScansController(ScanService scanService, RabbitMqPublisher publisher)
        {
            _scanService = scanService;
            _publisher = publisher;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateScanRequest request)
        {
            if (_scanService.DocumentIdExists(request.DocumentId))
            {
                return Conflict(new { error = $"A scan with documentId '{request.DocumentId}' already exists." });
            }

            var scan = _scanService.Create(request);
            
            _publisher.Publish(new ScanMessage { ScanId = scan.Id });

            return Accepted(new
            {
                id = scan.Id,
                status = scan.Status
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_scanService.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var scan = _scanService.GetById(id);

            if (scan == null)
            {
                return NotFound();
            }

            return Ok(scan);
        }

        [HttpGet("{id}/{text}")]
        public IActionResult ScansText(string id, string text)
        {
            var scan = _scanService.GetById(id);

            if (scan == null)
            {
                return NotFound();
            }

            bool areFound = _scanService.ContainsWordInText(id, text);

            return Ok(new
            {
                scanId = id,
                word = text,
                found = areFound
            });
        }
    }
}