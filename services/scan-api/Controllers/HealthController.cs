using Microsoft.AspNetCore.Mvc;

namespace scan_api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "scan-api",
                time = DateTime.UtcNow
            });
        }
    }
}