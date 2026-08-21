using Stripe.Checkout;
using System.Threading.Tasks;
namespace DreysStoreEcommerce.Services
{
  
    /// <summary>
    /// Service responsible for payment provider interactions (Stripe).
    /// </summary>
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IConfiguration config, ILogger<PaymentService> logger)
        {
            _config = config;
            _logger = logger;
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
    }

}
