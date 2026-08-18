using Microsoft.EntityFrameworkCore;
using OnlineShoppingApplication.Models;

namespace OnlineShoppingApplication.Data
{
    /// <summary>
    /// Database context for the online shopping application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing Tables
        public DbSet<Product> Products { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        // New Table
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================
            // Product Configuration
            // ==========================
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .Property(p => p.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ==========================
            // User Configuration
            // ==========================
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            // ==========================
            // Cart Configuration
            // ==========================
            modelBuilder.Entity<Cart>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Cart>()
                .Property(c => c.SessionId)
                .IsRequired();

            // ==========================
            // CartItem Configuration
            // ==========================
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.Id);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasPrecision(18, 2);

            // ==========================
            // Seed Products
            // ==========================
            SeedProducts(modelBuilder);
        }

        private void SeedProducts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                    Price = 75000,
                    Discount = 10,
                    Category = "Electronics",
                    StockQuantity = 50
                },
                new Product
                {
                    Id = 2,
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with 2.4GHz connection",
                    Price = 1500,
                    Discount = 5,
                    Category = "Accessories",
                    StockQuantity = 200
                },
                new Product
                {
                    Id = 3,
                    Name = "USB-C Cable",
                    Description = "Premium USB-C charging and data transfer cable",
                    Price = 800,
                    Discount = 0,
                    Category = "Accessories",
                    StockQuantity = 500
                },
                new Product
                {
                    Id = 4,
                    Name = "Mechanical Keyboard",
                    Description = "RGB mechanical keyboard with Cherry MX switches",
                    Price = 8500,
                    Discount = 8,
                    Category = "Accessories",
                    StockQuantity = 75
                },
                new Product
                {
                    Id = 5,
                    Name = "Portable SSD",
                    Description = "1TB portable SSD with USB-C connection, transfer speeds up to 1050MB/s",
                    Price = 12000,
                    Discount = 12,
                    Category = "Storage",
                    StockQuantity = 100
                },
                new Product
                {
                    Id = 6,
                    Name = "USB Hub",
                    Description = "7-port USB 3.0 hub with power adapter",
                    Price = 2500,
                    Discount = 5,
                    Category = "Accessories",
                    StockQuantity = 150
                },
                new Product
                {
                    Id = 7,
                    Name = "Monitor Stand",
                    Description = "Adjustable monitor stand with storage drawer",
                    Price = 3500,
                    Discount = 7,
                    Category = "Accessories",
                    StockQuantity = 80
                },
                new Product
                {
                    Id = 8,
                    Name = "Webcam",
                    Description = "1080p HD webcam with built-in microphone",
                    Price = 4500,
                    Discount = 10,
                    Category = "Electronics",
                    StockQuantity = 120
                }
            );
        }
    }
}
