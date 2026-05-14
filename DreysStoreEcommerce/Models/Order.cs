using DreysStoreEcommerce.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreysStoreEcommerce.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }  // ✅ correct column
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int? VendorId { get; set; }
        public Vendor Vendor { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        

        public ICollection<OrderItem> Items { get; set; }
    }


}
