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


        public DbSet<BudgetPlan> BudgetPlans { get; set; }
        public DbSet<BudgetPlanItem> BudgetPlanItems { get; set; }
        public DbSet<MealSuggestion> MealSuggestions { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users")
                .HasIndex(u => u.email)
                .IsUnique();

            // ProductDietaryTag configuration
            modelBuilder.Entity<ProductDietaryTag>()
                .HasKey(pd => pd.Id);

            // Link ProductDietaryTag to Commodity (instead of ProductPrice)
            modelBuilder.Entity<ProductDietaryTag>()
                .HasOne(pd => pd.Commodity)
                .WithMany(c => c.ProductDietaryTags)
                .HasForeignKey(pd => pd.CommodityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Link ProductDietaryTag to DietaryTag
            modelBuilder.Entity<ProductDietaryTag>()
                .HasOne(pd => pd.DietaryTag)
                .WithMany(d => d.ProductDietaryTags)
                .HasForeignKey(pd => pd.DietaryTagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint per commodity + tag
            modelBuilder.Entity<ProductDietaryTag>()
                .HasIndex(pd => new { pd.CommodityId, pd.DietaryTagId })
                .IsUnique();


            modelBuilder.Entity<UserFavorite>()
          .HasOne(uf => uf.User)
          .WithMany(u => u.Favorites)
          .HasForeignKey(uf => uf.UserId)
          .OnDelete(DeleteBehavior.Cascade);  // ⭐ AUTO DELETE WHEN USER IS DELETED

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Commodity)
                .WithMany(c => c.FavoritedByUsers) // ✅ Dito ang tamang property
                .HasForeignKey(uf => uf.CommodityId)
                .OnDelete(DeleteBehavior.Cascade);  // ⭐ AUTO DELETE WHEN COMMODITY IS DELETED

        }

    }
}
