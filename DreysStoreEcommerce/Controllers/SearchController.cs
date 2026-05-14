using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Mvc;


namespace DreysStoreEcommerce.Controllers
{
    
    public class SearchController : Controller
    {
        private readonly ProductService _productService;

        public SearchController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string keyword, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var results = await _productService.SearchProductsAsync(keyword, categoryId, minPrice, maxPrice);
            return View(results);
        }
    }

}
