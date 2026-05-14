using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DreysStoreEcommerce.Controllers
{
   

    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;

        public CartController(UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _userManager = userManager;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartVM = await _cartService.GetCartViewModelAsync(user.Id); // use ViewModel
            return View(cartVM);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.AddToCartAsync(user.Id, productId, quantity);
            return RedirectToAction("Index");
        }
        //public async Task<IActionResult> Index()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var cartVM = await _cartService.GetCartViewModelAsync(user.Id);
        //    return View(cartVM);
        //}

        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            await _cartService.ClearCartAsync(user.Id);
            return RedirectToAction("Index");
        }
    }

}
