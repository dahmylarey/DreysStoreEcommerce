using AutoMapper;
using AutoMapper.QueryableExtensions;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace DreysStoreEcommerce.Services
{
   
    public class WishlistService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(ApplicationDbContext context, IMapper mapper, ILogger<WishlistService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<WishlistItemViewModel>> GetWishlistItemsAsync(string userId)
        {
            _logger.LogInformation("Fetching wishlist items for user {UserId}", userId);
            return await _context.WishlistItems
                .Where(w => w.ApplicationUserId == userId)
                .Include(w => w.Product)
                .ProjectTo<WishlistItemViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(string userId, int productId)
        {
            var exists = await _context.WishlistItems
                .AnyAsync(w => w.ApplicationUserId == userId && w.ProductId == productId);
            if (!exists)
            {
                var item = new WishlistItem
                {
                    ApplicationUserId = userId,
                    ProductId = productId
                };
                await _context.WishlistItems.AddAsync(item);
                await _context.SaveChangesAsync();
            }
        }


        //public async Task AddToWishlistAsync(string userId, int productId)
        //{
        //    var exists = await _context.WishlistItems.AnyAsync(w => w.ApplicationUserId == userId && w.ProductId == productId);
        //    if (!exists)
        //    {
        //        _context.WishlistItems.Add(new DreysStoreEcommerce.Models.WishlistItem
        //        {
        //            ApplicationUserId = userId,
        //            ProductId = productId
        //        });
        //        await _context.SaveChangesAsync();
        //    }
        //}

        public async Task RemoveFromWishlistAsync(string userId, int productId)
        {
            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.ApplicationUserId == userId && w.ProductId == productId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }

}
