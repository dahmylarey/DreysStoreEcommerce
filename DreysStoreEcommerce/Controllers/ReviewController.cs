using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DreysStoreEcommerce.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ReviewService reviewService, UserManager<ApplicationUser> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, string comment, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            await _reviewService.AddReviewAsync(user.Id, productId, comment, rating);
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }

}
