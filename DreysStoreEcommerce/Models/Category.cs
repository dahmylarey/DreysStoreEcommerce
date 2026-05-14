using DreysStoreEcommerce.Models;

namespace DreysStoreEcommerce.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? VendorId { get; set; }
        public Vendor? Vendor { get; set; }
       
        
        public ICollection<Product> Products { get; set; }
    }
}
