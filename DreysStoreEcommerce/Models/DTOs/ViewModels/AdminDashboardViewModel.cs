using System.Collections.Generic;

namespace DreysStoreEcommerce.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalVendors { get; set; }

        public List<Product> RecentProducts { get; set; }
        public List<Order> RecentOrders { get; set; }
    }
}
