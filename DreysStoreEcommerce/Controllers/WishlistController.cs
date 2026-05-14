using DreysStoreEcommerce.Models;

namespace DreysStoreEcommerce.Controllers
{
    using DreysStoreEcommerce.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    
    public class WishlistController : Controller
    {
        private readonly WishlistService _wishlistService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(WishlistService wishlistService, UserManager<ApplicationUser> userManager)
        {
            _wishlistService = wishlistService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = await _wishlistService.GetWishlistItemsAsync(user.Id);
            return View(items);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "You must be logged in to add items to wishlist.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            await _wishlistService.AddToWishlistAsync(user.Id, productId);

            TempData["Success"] = "✅ Product added to your wishlist!";

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            await _wishlistService.RemoveFromWishlistAsync(user.Id, productId);
            return RedirectToAction("Index");
        }
    }


}
