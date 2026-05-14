using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Hubs.DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;
        private readonly ILogger<VendorController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext; // 🔔 Add hub context

        public VendorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            NotificationService notificationService,
            ILogger<VendorController> logger,
            IHubContext<NotificationHub> hubContext) // 🔔 inject hub
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
            _hubContext = hubContext; // 🔔
        }

        // GET: Vendor products
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> MyProducts()
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var products = await _context.Products
                                .Where(p => p.VendorId == vendor.Id)
                                .Include(p => p.Category)
                                .ToListAsync();

            return View(products);
        }

        // GET: Vendor Dashboard
        //[Authorize(Roles = "Admin,Vendor")]
        //public async Task<IActionResult> Dashboard()
        //{
        //    var vendor = await GetCurrentVendorAsync();
        //    if (vendor == null) return NotFound();

        //    var totalProducts = await _context.Products.CountAsync(p => p.VendorId == vendor.Id);
        //    var totalOrders = await _context.Orders.CountAsync(o => o.VendorId == vendor.Id);
        //    var pendingProducts = await _context.Products.CountAsync(p => p.VendorId == vendor.Id && !p.IsApproved);

        //    var model = new VendorDashboardViewModel
        //    {
        //        TotalProducts = totalProducts,
        //        TotalOrders = totalOrders,
        //        PendingProducts = pendingProducts
        //    };

        //    return View(model);
        //}

        // GET: Vendor Dashboard
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Dashboard()
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var totalProducts = await _context.Products.CountAsync(p => p.VendorId == vendor.Id);
            var totalOrders = await _context.Orders.CountAsync(o => o.VendorId == vendor.Id);
            var pendingProducts = await _context.Products.CountAsync(p => p.VendorId == vendor.Id && !p.IsApproved);

            // Create the DTO type the view expects
            var model = new DreysStoreEcommerce.Models.DTOs.ViewModels.VendorDashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                PendingProducts = pendingProducts
            };

            return View(model);
        }


        // GET: Create
        [Authorize(Roles = "Admin,Vendor")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Vendor create product
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Create(Product product)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            if (ModelState.IsValid)
            {
                product.VendorId = vendor.Id;
                _context.Add(product);
                await _context.SaveChangesAsync();

                var message = $"New product '{product.Name}' created by vendor.";
                await _notificationService.SendNotificationAsync(message);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔 broadcast live

                _logger.LogInformation("Vendor created product {ProductName} with ID {ProductId}", product.Name, product.Id);
                return RedirectToAction("MyProducts");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // GET: Edit
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Edit(int id)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (product == null) return NotFound();

            var message = $"Vendor is editing product '{product.Name}'.";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Vendor edit product {ProductName} with ID {ProductId}", product.Name, product.Id);

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (existingProduct == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.CategoryId = product.CategoryId;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    existingProduct.ImageUrl = "/images/products/" + fileName;
                }

                await _context.SaveChangesAsync();

                var message = $"Product '{existingProduct.Name}' has been updated by vendor.";
                await _notificationService.SendNotificationAsync(message);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
                _logger.LogInformation("Vendor updated product {ProductName} with ID {ProductId}", existingProduct.Name, existingProduct.Id);

                return RedirectToAction(nameof(MyProducts));
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // GET: Delete
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (product == null) return NotFound();

            var message = $"Product '{product.Name}' is about to be deleted by vendor.";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Vendor delete product {ProductName} with ID {ProductId}", product.Name, product.Id);

            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vendor = await GetCurrentVendorAsync();
            if (vendor == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendor.Id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                var message = $"Product '{product.Name}' has been deleted by vendor.";
                await _notificationService.SendNotificationAsync(message);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
                _logger.LogInformation("Vendor deleted product {ProductName} with ID {ProductId}", product.Name, product.Id);
            }

            return RedirectToAction(nameof(MyProducts));
        }

        // ViewModel
        //public class VendorDashboardViewModel
        //{
        //    public string VendorName { get; set; }
        //    public int ProductCount { get; set; }
        //    public int OrderCount { get; set; }
        //}

        public class VendorDashboardViewModel
        {
            public string VendorName { get; set; }
            public int TotalProducts { get; set; }
            public int TotalOrders { get; set; }
            public int PendingProducts { get; set; }
        }

        private async Task<Vendor> GetCurrentVendorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _context.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
        }
    }
}
