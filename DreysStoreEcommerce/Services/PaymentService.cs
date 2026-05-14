using Stripe.Checkout;
using System.Threading.Tasks;
namespace DreysStoreEcommerce.Services
{
  
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OrderService> _logger;

        public PaymentService(IConfiguration config, ILogger<OrderService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> CreateStripeCheckoutSession(decimal amount, string successUrl, string cancelUrl)
        {
            _logger.LogInformation("Creating Stripe checkout session for amount: {Amount}", amount);
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
