using System.ComponentModel.DataAnnotations;

namespace DreysStoreEcommerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Range(0.01, 999999, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        
        public bool IsApproved { get; set; } = false; 
        //  vendor
        public int? VendorId { get; set; }
        public Vendor Vendor { get; set; }
        //public ApplicationUser? Vendor { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }

}
