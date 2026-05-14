using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Hubs.DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.DTOs.ViewModels;
using DreysStoreEcommerce.Models.ViewModels;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;
        private readonly ILogger<AdminController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            NotificationService notificationService,
            ILogger<AdminController> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalVendors = await _context.Vendors.CountAsync()
            };
            return View(model);
        }

        public IActionResult ActivityLogs()
        {
            var logs = _context.ActivityLogs
                .Include(l => l.PerformedByUser)
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            return View(logs);
        }

        public async Task<IActionResult> Vendors()
        {
            var vendors = await _context.Vendors.ToListAsync();
            return View(vendors);
        }

        // GET: Create Vendor
        public IActionResult CreateVendor()
        {
            return View();
        }

        // POST: Create Vendor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVendor(CreateVendorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Vendor");

                    var vendor = new Vendor
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address = model.Address,
                        ApplicationUserId = user.Id
                    };
                    _context.Vendors.Add(vendor);

                    var adminUser = await _userManager.GetUserAsync(User);
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Action = "Create Vendor",
                        Description = $"Vendor '{vendor.Name}' created by {adminUser.Email}",
                        PerformedByUserId = adminUser.Id
                    });

                    await _context.SaveChangesAsync();

                    var message = $"New vendor '{vendor.Name}' created by admin.";
                    await _notificationService.SendNotificationAsync(message);

                    // 🔔 Broadcast live notification
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

                    _logger.LogInformation("Admin created vendor {VendorName} with ID {VendorId}", vendor.Name, vendor.Id);

                    return RedirectToAction(nameof(Vendors));
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // GET: Edit Vendor
        public async Task<IActionResult> EditVendor(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
                return NotFound();

            var message = $"Admin is editing vendor '{vendor.Name}' with ID {id}.";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Admin is editing vendor {VendorName} with ID {VendorId}", vendor.Name, vendor.Id);

            return View(vendor);
        }

        // POST: Edit Vendor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVendor(int id, Vendor vendor)
        {
            if (id != vendor.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(vendor);

                var adminUser = await _userManager.GetUserAsync(User);
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Action = "Edit Vendor",
                    Description = $"Vendor '{vendor.Name}' updated by {adminUser.Email}",
                    PerformedByUserId = adminUser.Id
                });

                await _context.SaveChangesAsync();

                var message = $"Vendor '{vendor.Name}' updated by admin.";
                await _notificationService.SendNotificationAsync(message);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
                _logger.LogInformation("Admin updated vendor {VendorName} with ID {VendorId}", vendor.Name, vendor.Id);

                return RedirectToAction(nameof(Vendors));
            }

            return View(vendor);
        }

        // GET: Delete Vendor
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
                return NotFound();

            _context.Vendors.Remove(vendor);

            var adminUser = await _userManager.GetUserAsync(User);
            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Delete Vendor",
                Description = $"Vendor '{vendor.Name}' deleted by {adminUser.Email}",
                PerformedByUserId = adminUser.Id
            });

            await _context.SaveChangesAsync();

            var message = $"Vendor '{vendor.Name}' deleted by admin.";
            await _notificationService.SendNotificationAsync(message);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message); // 🔔
            _logger.LogInformation("Admin deleted vendor {VendorName} with ID {VendorId}", vendor.Name, vendor.Id);

            return RedirectToAction(nameof(Vendors));
        }

        // Users, Orders, Products
        [Authorize(Roles = "Admin,Vendor")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders.Include(o => o.ApplicationUser).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Notifications with filter & pagination
        public async Task<IActionResult> Notifications(string filter = "all", int page = 1, int pageSize = 10)
        {
            IQueryable<Notification> query = _context.Notifications;

            if (filter == "unread")
                query = query.Where(n => !n.IsRead);
            else if (filter == "read")
                query = query.Where(n => n.IsRead);

            var totalCount = await query.CountAsync();
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Filter = filter;

            return View(notifications);
        }

        // POST: Mark notification as read
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var note = await _context.Notifications.FindAsync(id);
            if (note != null)
            {
                note.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Notifications));
        }

        // POST: Delete notification
        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var note = await _context.Notifications.FindAsync(id);
            if (note != null)
            {
                _context.Notifications.Remove(note);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Notifications));
        }

        // ViewModel
        public class AdminDashboardViewModel
        {
            public int TotalProducts { get; set; }
            public int TotalOrders { get; set; }
            public int TotalUsers { get; set; }
            public int TotalVendors { get; set; }
            public List<Product> RecentProducts { get; set; }
            public List<Order> RecentOrders { get; set; }
        }
    }
}
