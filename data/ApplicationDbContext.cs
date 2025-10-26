using Microsoft.EntityFrameworkCore;
using WebApplication2.models;
namespace WebApplication2.data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Games> Games { get; set; }


        public DbSet<Commodity> Commodities { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<PriceReport> PriceReports { get; set; }

        public DbSet<DietaryTag> DietaryTags { get; set; }
        public DbSet<ProductDietaryTag> ProductDietaryTags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users")
                .HasIndex(u => u.email)
                .IsUnique();


            modelBuilder.Entity<ProductDietaryTag>()
         .HasKey(pd => pd.Id);

            modelBuilder.Entity<ProductDietaryTag>()
                .HasOne(pd => pd.ProductPrice)
                .WithMany(p => p.ProductDietaryTags)
                .HasForeignKey(pd => pd.ProductPriceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDietaryTag>()
                .HasOne(pd => pd.DietaryTag)
                .WithMany(d => d.ProductDietaryTags)
                .HasForeignKey(pd => pd.DietaryTagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDietaryTag>()
                .HasIndex(pd => new { pd.ProductPriceId, pd.DietaryTagId })
                .IsUnique();
        }
    }
}
