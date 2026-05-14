using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DreysStoreEcommerce.Controllers
{
   

    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private readonly IEmailSender _emailSender;

        public CheckoutController(UserManager<ApplicationUser> userManager, CartService cartService, OrderService orderService, PaymentService paymentService, IEmailSender emailSender)
        {
            _userManager = userManager;
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _cartService.GetCartItemsAsync(user.Id);
            return View(cartItems);
        }

        
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


        [HttpPost]
        public async Task<IActionResult> ProcessOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _cartService.GetCartItemsAsync(user.Id);

            if (!cartItems.Any())
                return RedirectToAction("Index");

            var order = await _orderService.CreateOrderAsync(user, cartItems);
            await _cartService.ClearCartAsync(user.Id);

            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", $"Your order #{order.Id} has been placed.");

            // Redirect to Stripe checkout
            var successUrl = Url.Action("Success", "Checkout", null, Request.Scheme);
            var cancelUrl = Url.Action("Index", "Checkout", null, Request.Scheme);
            var stripeUrl = await _paymentService.CreateStripeCheckoutSession(order.TotalAmount, successUrl, cancelUrl);

            return Redirect(stripeUrl);
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }

}
