using AutoMapper;
using AutoMapper.QueryableExtensions;
using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.DTOs.ViewModels;
using DreysStoreEcommerce.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, IWebHostEnvironment env, IMapper mapper, ILogger<ProductService> logger)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
            _logger = logger;
        }

        //Add new product (with image upload)
        public async Task AddAsync(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Ensure the wwwroot/images/products folder exists
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Create unique filename
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Set ImageUrl to relative path
                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }

            // Add to database
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }



        //Get all products (with category)
        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        //Get product by ID
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        //Create new product + upload image
        public async Task<bool> CreateAsync(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return true;
        }

        //Update product + optionally upload new image
        public async Task<bool> UpdateAsync(Product updatedProduct, IFormFile newImageFile)
        {
            var product = await _context.Products.FindAsync(updatedProduct.Id);
            if (product == null)
                return false;

            // Update fields
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Description = updatedProduct.Description;
            product.CategoryId = updatedProduct.CategoryId;

            if (newImageFile != null && newImageFile.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);
                }

                // Save new image
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newImageFile.FileName);
                var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await newImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/products/{uniqueFileName}";
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }





        public async Task<bool> DeleteAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            // Delete image file if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<IEnumerable<Product>> SearchAsync(string query, int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(query))
                products = products.Where(p => p.Name.Contains(query));

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);

            return await products.ToListAsync();
        }

        public async Task<List<ProductViewModel>> SearchProductsAsync(string keyword, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            _logger.LogInformation("Searching products with keyword: {Keyword}, categoryId: {CategoryId}, minPrice: {MinPrice}, maxPrice: {MaxPrice}",
                keyword, categoryId, minPrice, maxPrice);

            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            return await query
                .ProjectTo<ProductViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }


        public async Task<ProductDetailsViewModel> GetDetailsByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return null;

            // Map Reviews to ReviewViewModel (if you have AutoMapper, you can use it)
            var reviewViewModels = product.Reviews?.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Comment = r.Comment,
                UserName = r.User?.FullName ?? "Anonymous"
            }).ToList() ?? new List<ReviewViewModel>();

            var vm = new ProductDetailsViewModel
            {
                Product = product,
                Reviews = reviewViewModels
                // WishlistItems: usually you don't preload here unless per-user
            };

            return vm;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Make sure /images/products folder exists
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return the relative URL to store in DB
            return "/images/products/" + uniqueFileName;
        }
    }
}
