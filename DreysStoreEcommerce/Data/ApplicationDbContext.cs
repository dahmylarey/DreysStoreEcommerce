using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fix IdentityUserToken max length issue
            builder.Entity<IdentityUserToken<string>>()
                .Property(p => p.Name).HasMaxLength(128);

            // Vendor → ApplicationUser (one-to-one)
            builder.Entity<Vendor>()
                .HasOne(v => v.ApplicationUser)
                .WithOne() // no navigation property in ApplicationUser; if you add: .WithOne(u => u.VendorProfile)
                .HasForeignKey<Vendor>(v => v.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Vendor (many-to-one)
            builder.Entity<Product>()
                .HasOne(p => p.Vendor)
                .WithMany(v => v.Products)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Category
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → Vendor
            builder.Entity<Order>()
                .HasOne(o => o.Vendor)
                .WithMany()
                .HasForeignKey(o => o.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → ApplicationUser
            builder.Entity<Order>()
                .HasOne(o => o.ApplicationUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order → OrderItems
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem → Product
            builder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem → Product
            builder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem → ApplicationUser
            builder.Entity<CartItem>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // WishlistItem → Product
            builder.Entity<WishlistItem>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // WishlistItem → ApplicationUser
            builder.Entity<WishlistItem>()
                .HasOne(w => w.ApplicationUser)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review → Product
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review → ApplicationUser
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Log → ApplicationUser
            builder.Entity<ActivityLog>()
            .HasOne(a => a.PerformedByUser)
            .WithMany()
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
