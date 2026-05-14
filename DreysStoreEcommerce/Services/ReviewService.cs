
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace DreysStoreEcommerce.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, IMapper mapper, ILogger<ReviewService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddReviewAsync(string userId, int productId, string comment, int rating)
        {
            var review = new Review
            {
                ApplicationUserId = userId,
                ProductId = productId,
                Comment = comment,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User {userId} added review for product {productId}");
        }


        public async Task<List<ReviewViewModel>> GetReviewsByProductIdAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return reviews.Select(r => new ReviewViewModel
            {
                UserName = r.User.FullName,
                Comment = r.Comment,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<Product>> SearchProductsAsync(string search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.ToListAsync();
        }

    }
}


