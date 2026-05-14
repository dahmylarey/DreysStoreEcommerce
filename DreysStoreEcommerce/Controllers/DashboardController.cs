using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DreysStoreEcommerce.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            

            // vendor.ApplicationUserId is string, user.Id is string → valid
            var vendor = _context.Vendors.FirstOrDefault(v => v.ApplicationUserId == user.Id);

            if (vendor == null)
            {
                return RedirectToAction("Register", "VendorProfile");
            }

            // vendor.Id is int, p.VendorId is int → valid
            var productCount = _context.Products.Count(p => p.VendorId == vendor.Id);
            var orderCount = _context.Orders.Count(o => o.VendorId == vendor.Id);
            var totalRevenue = _context.Orders
                .Where(o => o.VendorId == vendor.Id)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;


            var model = new VendorDashboardViewModel
            {
                ProductCount = productCount,
                OrderCount = orderCount,
                TotalRevenue = totalRevenue
            };

            return View(model);
        }


    }

    public class VendorDashboardViewModel
    {
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
