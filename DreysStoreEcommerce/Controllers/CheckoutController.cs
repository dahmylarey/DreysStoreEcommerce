using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DreysStoreEcommerce.Controllers
{
   

    /// <summary>
    /// Controller that handles checkout flow: viewing checkout, creating orders and initiating payment.
    /// </summary>
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(UserManager<ApplicationUser> userManager, CartService cartService, OrderService orderService, PaymentService paymentService, IEmailSender emailSender, ILogger<CheckoutController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _cartService.GetCartItemsAsync(user.Id);
            return View(cartItems);
        }

        
        /// <summary>
        /// Create an order from the user's cart and redirect to the success page.
        /// This action is kept for compatibility with existing checkout flows.
        /// </summary>
        public async Task<IActionResult> Process()
            {
                var user = await _userManager.GetUserAsync(User);
                var orderId = await _orderService.CheckoutAsync(user.Id);
                if (orderId == -1)
                {
                    ModelState.AddModelError("", "Cart is empty.");
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Success", new { id = orderId });
            }


        /// <summary>
        /// Process the order: create order record, clear cart, attempt to send confirmation email,
        /// and redirect to the configured payment provider.
        /// This method will not throw on email failures and will show a friendly message if payment
        /// provider initialization fails.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProcessOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _cartService.GetCartItemsAsync(user.Id);

            if (!cartItems.Any())
                return RedirectToAction("Index");
            var order = await _orderService.CreateOrderAsync(user, cartItems);
            await _cartService.ClearCartAsync(user.Id);

            // If order total is zero, skip payment and inform user
            if (order.TotalAmount <= 0)
            {
                TempData["Error"] = "Order total is zero. Please add items with valid prices to your cart before checking out.";
                return RedirectToAction("Index");
            }

            // Send confirmation email but don't fail checkout if mail fails
            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", $"Your order #{order.Id} has been placed.");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Email sending failed for order {OrderId}; continuing checkout.", order.Id);
            }

            // Prefer Paystack when configured (e.g. NGN/local). Fall back to Stripe if Paystack is not configured.
            var successUrl = Url.Action("Success", "Checkout", null, Request.Scheme);
            var cancelUrl = Url.Action("Index", "Checkout", null, Request.Scheme);

            if (_paymentService.IsPaystackConfigured())
            {
                try
                {
                    var paystackUrl = await _paymentService.CreatePaystackCheckoutSession(order.TotalAmount, user.Email, successUrl, order.Id);
                    return Redirect(paystackUrl);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to create Paystack checkout session for order {OrderId}", order.Id);
                    TempData["ErrorMessage"] = "There was a problem initializing the Paystack payment gateway. Please try again later.";
                    return RedirectToAction("Index");
                }
            }

            // Redirect to Stripe as fallback
            try
            {
                var stripeUrl = await _paymentService.CreateStripeCheckoutSession(order.TotalAmount, successUrl, cancelUrl);
                return Redirect(stripeUrl);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create Stripe checkout session for order {OrderId}", order.Id);
                TempData["ErrorMessage"] = "There was a problem initializing the payment gateway. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }

}
