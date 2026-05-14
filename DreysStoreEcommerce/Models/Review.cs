namespace DreysStoreEcommerce.Models
{



    public class Review
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string Comment { get; set; }
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }
    }


}
