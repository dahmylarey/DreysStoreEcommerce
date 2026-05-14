using DreysStoreEcommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DreysStoreEcommerce.Data
{
    public static class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context,
                                      UserManager<ApplicationUser> userManager,
                                      RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // === Seed roles ===
            string[] roles = new[] { "Admin", "Customer", "Vendor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // === Seed default admin user ===
            string adminEmail = "admin@dreystore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Address = "HQ",
                    // Add other properties if you have e.g., FullName
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // === Seed default vendor user ===
            string vendorEmail = "vendor@dreystore.com";
            var vendorUser = await userManager.FindByEmailAsync(vendorEmail);
            if (vendorUser == null)
            {
                vendorUser = new ApplicationUser
                {
                    UserName = vendorEmail,
                    Email = vendorEmail,
                    Address = "Vendor Address",
                    // Add other properties
                };
                var result = await userManager.CreateAsync(vendorUser, "Vendor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendorUser, "Vendor");
                }
            }

            // === Seed Vendor entity linked to vendorUser ===
            var vendor = await context.Vendors.FirstOrDefaultAsync(v => v.ApplicationUserId == vendorUser.Id);
            if (vendor == null)
            {
                vendor = new Vendor
                {
                    Name = "Sample Vendor",
                    Email = vendorUser.Email,
                    Address = "Vendor Address",
                    ApplicationUserId = vendorUser.Id,
                    IsActive = true
                };
                context.Vendors.Add(vendor);
                await context.SaveChangesAsync();
            }

            // === Seed sample categories ===
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "Electronics" },
                    new Category { Name = "Fashion" },
                    new Category { Name = "Home & Living" },
                    new Category { Name = "Beauty & Personal Care" },
                    new Category { Name = "Sports & Outdoors" },
                    new Category { Name = "Books, Music & Media" },
                    new Category { Name = "Toys & Games" },
                    new Category { Name = "Groceries & Food" },
                    new Category { Name = "Health & Wellness" },
                    new Category { Name = "Automotive & Tools" },
                    new Category { Name = "Jewelry & Watches" },
                    new Category { Name = "Pet Supplies" },
                    new Category { Name = "Baby & Kids" },
                    new Category { Name = "Office Supplies" },
                    new Category { Name = "Garden & Outdoor" },
                    new Category { Name = "Arts & Crafts" },


                });
                await context.SaveChangesAsync();
            }

            // === Seed sample product ===
            if (!context.Products.Any())
            {
                var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
                context.Products.Add(new Product
                {
                    Name = "Smart Phone",
                    Description = "Example description",
                    Price = 299.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample.jpg",
                    IsApproved = true
                });

                context.Products.Add(new Product
                {
                    Name = "Dell Laptop",
                    Description = "Example description",
                    Price = 999.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/Dell-laptop.jpg",
                    IsApproved = true
                });

                context.Products.Add(new Product
                {
                    Name = "Headphones",
                    Description = "Example description",
                    Price = 49.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample-headphones.jpg",
                    IsApproved = true
                });
                context.Products.Add(new Product
                {
                    Name = "Tablet",
                    Description = "Example description",
                    Price = 199.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample-tablet.jpg",
                    IsApproved = true
                });
                context.Products.Add(new Product
                {
                    Name = "Smartwatch",
                    Description = "Example description",
                    Price = 149.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/smartwatch.jpg",
                    IsApproved = true
                });

                context.Products.Add(new Product
                {
                    Name = "Camera",
                    Description = "Example description",
                    Price = 499.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample-camera.jpg",
                    IsApproved = true
                });

                context.Products.Add(new Product
                {
                    Name = "Bluetooth Speaker",
                    Description = "Example description",
                    Price = 79.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/Bluetooth-speaker.jpg",
                    IsApproved = true
                });
                context.Products.Add(new Product
                {
                    Name = "Smart TV",
                    Description = "Example description",
                    Price = 799.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample-tv.jpg",
                    IsApproved = true
                });
                context.Products.Add(new Product
                {
                    Name = "Gaming Console",
                    Description = "Example description",
                    Price = 399.99M,
                    CategoryId = electronics.Id,
                    VendorId = vendor.Id, // ✅ now correct: vendor.Id is int
                    ImageUrl = "/images/products/sample-console.jpg",
                    IsApproved = true
                });




                await context.SaveChangesAsync();
            }
        }
    }
}
