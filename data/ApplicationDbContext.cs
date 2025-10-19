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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users")
                .HasIndex(u => u.email)
                .IsUnique();
        }
    }
}
