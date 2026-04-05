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
        private readonly ScanQueueService _queue;

        public ScansController(ScanService scanService, ScanQueueService queue)
        {
            _scanService = scanService;
            _queue = queue;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateScanRequest request)
        {
            var scan = _scanService.Create(request);
            
            _queue.Enqueue(new ScanMessage { ScanId = scan.Id });

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

            bool areFound = _scanService.ContainsWord(id, text);

            return Ok(new
            {
                scanId = id,
                word = text,
                found = areFound
            });
        }
    }
}