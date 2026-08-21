using Stripe.Checkout;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Globalization;
using System;
namespace DreysStoreEcommerce.Services
{
  
    /// <summary>
    /// Service responsible for payment provider interactions (Stripe).
    /// </summary>
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentService(IConfiguration config, ILogger<PaymentService> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Returns true when a Stripe secret key is configured in application configuration.
        /// Checks configuration key "Stripe:SecretKey" and returns whether it is present.
        /// </summary>
        /// <returns>True when Stripe secret key exists.</returns>
        public bool IsConfigured()
        {
            var key = _config["Stripe:SecretKey"];
            return !string.IsNullOrEmpty(key);
        }

        /// <summary>
        /// Returns true when Paystack secret key is configured.
        /// </summary>
        public bool IsPaystackConfigured()
        {
            var key = _config["Paystack:SecretKey"];
            return !string.IsNullOrEmpty(key);
        }

        /// <summary>
        /// Create a Stripe Checkout Session and return the URL to redirect the browser to.
        /// Throws informative exceptions when input is invalid or Stripe is not configured.
        /// </summary>
        /// <param name="amount">Total amount (decimal) in the configured currency.</param>
        /// <param name="successUrl">Return URL when payment succeeds.</param>
        /// <param name="cancelUrl">Return URL when payment is cancelled.</param>
        /// <returns>A task that resolves to the Stripe Checkout Session URL.</returns>
        public async Task<string> CreateStripeCheckoutSession(decimal amount, string successUrl, string cancelUrl)
        {
            _logger.LogInformation("Creating Stripe checkout session for amount: {Amount}", amount);
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero to create a checkout session.");
            }
            var key = _config["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Stripe secret key is not configured.");
            }
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(amount * 100), // Stripe uses cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Drey Store Order"
                        }
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        /// <summary>
        /// Initialize a Paystack transaction and return the authorization URL.
        /// </summary>
        /// <param name="amount">Order total amount.</param>
        /// <param name="email">Customer email address.</param>
        /// <param name="callbackUrl">Paystack callback URL.</param>
        /// <param name="orderId">Order id to include in metadata.</param>
        /// <returns>Authorization URL to redirect the user to Paystack checkout.</returns>
        public async Task<string> CreatePaystackCheckoutSession(decimal amount, string email, string callbackUrl, int orderId)
        {
            _logger.LogInformation("Initializing Paystack transaction for order {OrderId} amount {Amount}", orderId, amount);
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");

            var key = _config["Paystack:SecretKey"];
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Paystack secret key is not configured.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var payload = new
            {
                email = email,
                amount = (long)(amount * 100m),
                callback_url = callbackUrl,
                metadata = new { order_id = orderId }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.paystack.co/transaction/initialize", content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paystack initialize failed: {Status} {Body}", response.StatusCode, body);
                throw new InvalidOperationException("Failed to initialize Paystack transaction.");
            }

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("authorization_url", out var authUrl))
            {
                return authUrl.GetString();
            }

            _logger.LogError("Paystack initialize response missing authorization_url: {Body}", body);
            throw new InvalidOperationException("Unexpected Paystack response.");
        }
    }

}
