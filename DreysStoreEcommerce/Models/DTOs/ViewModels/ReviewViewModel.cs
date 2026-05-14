namespace DreysStoreEcommerce.Models.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
