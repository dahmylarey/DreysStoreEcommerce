using Microsoft.AspNetCore.Identity;
using System;

namespace DreysStoreEcommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string? ProfilePictureUrl { get; set; }


        public int? VendorId { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<WishlistItem> WishlistItems { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
