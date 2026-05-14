using System;

namespace DreysStoreEcommerce.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; }  // optional: send to specific user
        public bool IsRead { get; set; } = false;
        public string ApplicationUserId { get; set; } // FK to recipient user
        
    }
}
