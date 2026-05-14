using DreysStoreEcommerce.Models.ViewModels;
using System.Collections.Generic;

namespace DreysStoreEcommerce.Models.DTOs.ViewModels
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; }


        public Product Product { get; set; }            // ✅ the main product
          
        // Reviews if you have them
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
    }
}
