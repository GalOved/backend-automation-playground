using Microsoft.AspNetCore.Mvc;
using webhook_receiver.Models;

namespace webhook_receiver.Controllers
{
    [ApiController]
    [Route("webhooks")]
    public class WebhooksController : ControllerBase
    {
        private static readonly List<WebhookPayload> _webhooks = new();

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_webhooks);
        }

        [HttpPost("status")]
        public IActionResult AddWebhook([FromBody] WebhookPayload payload)
        {
            _webhooks.Add(payload);
            return Ok(new { message = "Webhook received.", scanId = payload.ScanId });
        }
    }    
}