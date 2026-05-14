using AutoMapper;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

public class CartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CartService> _logger;
    private readonly IMapper _mapper;

    public CartService(ApplicationDbContext context, ILogger<CartService> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task AddToCartAsync(string userId, int productId, int quantity)
    {
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            _logger.LogInformation($"Updated quantity for product {productId} in cart of user {userId}");
        }
        else
        {
            var item = new CartItem
            {
                ApplicationUserId = userId,
                ProductId = productId,
                Quantity = quantity
            };
            await _context.CartItems.AddAsync(item);
            _logger.LogInformation($"Added product {productId} to cart of user {userId}");
        }

        await _context.SaveChangesAsync();


        _logger.LogInformation("Cart updated successfully for user {UserId}", userId);
    }


    public async Task<List<CartItem>> GetCartItemsAsync(string userId)
    {
        _logger.LogInformation("Fetching raw cart items for user {UserId}", userId);
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.ApplicationUserId == userId)
            .ToListAsync();

        //return _mapper.Map<List<CartItemViewModel>>(items);
    }

    public async Task<List<CartItemViewModel>> GetCartViewModelAsync(string userId)
    {
        var items = await GetCartItemsAsync(userId);
        return _mapper.Map<List<CartItemViewModel>>(items);
    }


    public async Task ClearCartAsync(string userId)
    {
        _logger.LogInformation("Clearing cart for user {UserId}", userId);
        var items = await _context.CartItems.Where(c => c.ApplicationUserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
