using System.ComponentModel.DataAnnotations;

namespace DreysStoreEcommerce.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Phone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; } = true;
        public string Email { get; set; }
        public string? LogoUrl { get; set; }


       
        // FK to ApplicationUser
        public string? ApplicationUserId { get; set; }
        //public ApplicationUser? User { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        // Products owned by this vendor
        public ICollection<Product>? Products { get; set; }
    }
}

