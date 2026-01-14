using Microsoft.EntityFrameworkCore;
using SuiQuickRecorderWebAPI.Entities;

namespace SuiQuickRecorderWebAPI.Data
{
    public class SuiContext : DbContext
    {
        public SuiContext(DbContextOptions<SuiContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<CategoryIn> CategoriesIn { get; set; }
        public DbSet<CategoryOut> CategoriesOut { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Loaner> Loaners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("accounts");
            modelBuilder.Entity<CategoryIn>().ToTable("categories_in");
            modelBuilder.Entity<CategoryOut>().ToTable("categories_out");
            modelBuilder.Entity<Store>().ToTable("stores");
            modelBuilder.Entity<Loaner>().ToTable("loaners");
        }
    }
}
