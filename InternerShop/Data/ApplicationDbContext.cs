using InternerShop.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        // Добавьте в ApplicationDbContext
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Дополнительные конфигурации моделей
            builder.Entity<Discount>(entity =>
            {
                entity.Property(d => d.DiscountPercentage)
                    .HasColumnType("decimal(5,2)");
            });

            builder.Entity<ProductDiscount>()
                .HasKey(pd => pd.ProductDiscountId);

            builder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.ProductDiscounts)
                .HasForeignKey(pd => pd.ProductId);

            builder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Discount)
                .WithMany(d => d.ProductDiscounts)
                .HasForeignKey(pd => pd.DiscountId);

            // Конфигурация для ApplicationUser
            builder.Entity<ApplicationUser>(b =>
            {
                b.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId);

                b.HasMany(u => u.Reviews)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId);

                b.HasMany(u => u.UserAddresses)
                    .WithOne(ua => ua.User)
                    .HasForeignKey(ua => ua.UserId);
            });
        }
    }
}
