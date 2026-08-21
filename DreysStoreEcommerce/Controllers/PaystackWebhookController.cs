using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DreysStoreEcommerce.Controllers
{
    /// <summary>
    /// Handles Paystack webhook events. Configure your Paystack callback URL to point here.
    /// </summary>
    [Route("api/paystack")]
    [ApiController]
    public class PaystackWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly OrderService _orderService;
        private readonly ILogger<PaystackWebhookController> _logger;

        public PaystackWebhookController(IConfiguration config, ApplicationDbContext db, OrderService orderService, ILogger<PaystackWebhookController> logger)
        {
            _config = config;
            _db = db;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            var secret = _config["Paystack:SecretKey"];
            var signature = Request.Headers["x-paystack-signature"].FirstOrDefault();
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(secret))
            {
                // Verify signature
                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                if (string.IsNullOrEmpty(signature) || !string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid Paystack webhook signature.");
                    return Unauthorized();
                }
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var eventType = root.GetProperty("event").GetString();
                _logger.LogInformation("Received Paystack event: {Event}", eventType);

                if (root.TryGetProperty("data", out var data))
                {
                    // Paystack includes metadata with order_id if we set it
                    int orderId = 0;
                    if (data.TryGetProperty("metadata", out var metadata) && metadata.TryGetProperty("order_id", out var orderIdElem))
                    {
                        if (orderIdElem.ValueKind == JsonValueKind.Number && orderIdElem.TryGetInt32(out var id))
                            orderId = id;
                        else if (orderIdElem.ValueKind == JsonValueKind.String && int.TryParse(orderIdElem.GetString(), out var id2))
                            orderId = id2;
                    }

                    if (!string.IsNullOrEmpty(eventType) && eventType == "charge.success")
                    {
                        if (orderId > 0)
                        {
                            await _orderService.UpdateOrderStatusAsync(orderId, "Paid");
                            _logger.LogInformation("Order {OrderId} marked as Paid via Paystack webhook.", orderId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Paystack webhook");
                return BadRequest();
            }

            return Ok();
        }
    }
}
