namespace DreysStoreEcommerce.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string Action { get; set; }             // e.g., "Create Vendor"
        public string Description { get; set; }        // e.g., "Vendor 'Acme' created by admin@example.com"
        public string PerformedByUserId { get; set; }  // FK to ApplicationUser.Id
        public ApplicationUser PerformedByUser { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
