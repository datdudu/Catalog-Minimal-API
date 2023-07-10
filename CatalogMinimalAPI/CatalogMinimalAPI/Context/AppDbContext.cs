using CatalogMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogMinimalAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { }

        public DbSet<Product>? Products { get; set; }
        public DbSet<Category>? Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            //Categories
            mb.Entity<Category>().HasKey(c => c.CategoryId);

            mb.Entity<Category>().Property(c => c.Name)
                                 .HasMaxLength(100)
                                 .IsRequired();

            mb.Entity<Category>().Property(c => c.Description)
                                 .HasMaxLength(150)
                                 .IsRequired();

            //Products
            mb.Entity<Product>().HasKey(c => c.ProductId);

            mb.Entity<Product>().Property(p => p.Name)
                                .HasMaxLength(100)
                                .IsRequired();

            mb.Entity<Product>().Property(p => p.Description)
                                .HasMaxLength(150);

            mb.Entity<Product>().Property(p => p.Image)
                                .HasMaxLength(100);

            mb.Entity<Product>().Property(p => p.Price)
                                .HasPrecision(14, 2);

            //Relationship
            mb.Entity<Product>()
                .HasOne<Category>(c => c.Category)
                    .WithMany(p => p.Products)
                        .HasForeignKey(c => c.CategoryId);
        }
    }
}
