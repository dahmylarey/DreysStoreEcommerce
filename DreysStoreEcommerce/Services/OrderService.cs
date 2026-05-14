using AutoMapper;
using AutoMapper.QueryableExtensions;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreysStoreEcommerce.Services
{
   
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        //public async Task<List<OrderViewModel>> GetUserOrdersAsync(string userId)
        //{
        //    var orders = await _context.Orders
        //        .Include(o => o.Items).ThenInclude(i => i.Product)
        //        .Include(o => o.ApplicationUser)
        //        .Where(o => o.ApplicationUserId == userId)
        //        .ToListAsync();

        //    return _mapper.Map<List<OrderViewModel>>(orders);
        //}


        public async Task<List<OrderViewModel>> GetOrdersByUserAsync(string userId)
        {
            _logger.LogInformation("Fetching orders for user {UserId}", userId);

            var orders = await _context.Orders
       .Include(o => o.Items).ThenInclude(i => i.Product)
       .Include(o => o.ApplicationUser)
       .Where(o => o.ApplicationUserId == userId)
       .ToListAsync();

            return _mapper.Map<List<OrderViewModel>>(orders);

            
        }

        public async Task<Order> CreateOrderAsync(ApplicationUser user, List<CartItem> cartItems)
        {
            _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items", user.Id, cartItems.Count);
            var order = new Order
            {
                ApplicationUserId = user.Id,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Pending",
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task ClearCartAsync(string userId)
        {
            _logger.LogInformation("Clearing cart for user {UserId}", userId);
            var items = _context.CartItems.Where(c => c.ApplicationUserId == userId);
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }


        public async Task<int> CheckoutAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == userId)
                .ToListAsync();

            if (!cartItems.Any()) return -1;

            var order = new Order
            {
                ApplicationUserId = userId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                Status = "Pending",
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToList()
            };

            await _context.Orders.AddAsync(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.Id} created for user {userId}");
            return order.Id;
        }
    }

}
