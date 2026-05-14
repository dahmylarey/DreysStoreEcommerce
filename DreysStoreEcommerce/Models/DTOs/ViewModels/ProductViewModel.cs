namespace DreysStoreEcommerce.Models.DTOs.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public Product Product { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        
        public string? CategoryName { get; set; }  // comes from related Category
    }
}
