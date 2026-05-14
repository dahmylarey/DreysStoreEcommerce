using System;
using System.Collections.Generic;

namespace DreysStoreEcommerce.Models.ViewModels
{

    public class OrderViewModel
    {
        public int Id { get; set; }                  
        public DateTime OrderDate { get; set; }      //mapped from CreatedAt
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public string UserEmail { get; set; }        // optional, mapped if you need
        public ICollection<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
