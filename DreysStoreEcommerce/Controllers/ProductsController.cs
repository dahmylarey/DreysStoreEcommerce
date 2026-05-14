using AutoMapper;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Hubs.DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.DTOs;
using DreysStoreEcommerce.Models.DTOs.ViewModels;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;
        private readonly CartService _cartService;
        private readonly WishlistService _wishlistService;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<ProductsController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext; // 🔔 Add hub context

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ProductService productService,
            ReviewService reviewService,
            CartService cartService,
            WishlistService wishlistService,
            IMapper mapper,
            ICategoryService categoryService,
            NotificationService notificationService,
            ILogger<ProductsController> logger,
            IHubContext<NotificationHub> hubContext) // 🔔 inject hub
        {
            _context = context;
            _userManager = userManager;
            _productService = productService;
            _reviewService = reviewService;
            _cartService = cartService;
            _wishlistService = wishlistService;
            _mapper = mapper;
            _categoryService = categoryService;
            _notificationService = notificationService;
            _logger = logger;
            _hubContext = hubContext; // 🔔
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductViewModel
            {
                Product = new Product(),
                Categories = await _categoryService.GetAllAsync()
            };
            return View(vm);
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            vm.Categories = await _categoryService.GetAllAsync();

            await _productService.AddAsync(vm.Product, vm.ImageFile);

            var message = $"New product '{vm.Product.Name}' created by admin.";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔 broadcast
            _logger.LogInformation("Admin created product {ProductName} with ID {ProductId}", vm.Product.Name, vm.Product.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _productService.GetDetailsByIdAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var message = $"Admin is editing product '{product.Name}' with ID {product.Id}";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Admin is editing product {ProductName} with ID {ProductId}", product.Name, product.Id);

            var vm = new ProductViewModel
            {
                Product = product,
                Categories = await _categoryService.GetAllAsync()
            };
            return View(vm);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile ImageFile)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var updated = await _productService.UpdateAsync(product, ImageFile);
                if (!updated)
                    return NotFound();

                var message = $"Admin updated product '{product.Name}' with ID {product.Id}";
                await _notificationService.SendNotificationAsync(message);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
                _logger.LogInformation("Admin updated product {ProductName} with ID {ProductId}", product.Name, product.Id);

                return RedirectToAction(nameof(Index));
            }

            var vm = new ProductViewModel
            {
                Product = product,
                Categories = await _categoryService.GetAllAsync()
            };
            return View(vm);
        }

        // GET: /Products/Delete/5
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var message = $"Admin is about to delete product '{product.Name}' with ID {product.Id}";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Admin is deleting product {ProductName} with ID {ProductId}", product.Name, product.Id);

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            var message = $"Admin deleted product '{product.Name}' with ID {product.Id}";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Admin deleted product {ProductName} with ID {ProductId}", product.Name, product.Id);

            return RedirectToAction(nameof(Index));
        }

        // POST: Products/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, string content, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _reviewService.AddReviewAsync(user.Id, productId, content, rating);
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // GET: /Products/Search
        [AllowAnonymous]
        public async Task<IActionResult> Search(string q, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(q))
                products = products.Where(p => p.Name.Contains(q) || p.Description.Contains(q));

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            var result = await products.Include(p => p.Category).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(result);
        }

        // POST: Products/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _cartService.AddToCartAsync(user.Id, productId, quantity);
            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}
