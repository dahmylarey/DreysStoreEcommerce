using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace DreysStoreEcommerce.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendNotificationAsync(string message, string userId = null)
        {
            var notification = new Notification
            {
                Message = message,
                UserId = userId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
