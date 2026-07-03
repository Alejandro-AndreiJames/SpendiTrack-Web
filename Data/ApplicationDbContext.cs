using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserBudget>()
                .HasIndex(b => new { b.UserId, b.Year, b.Month })
                .IsUnique();

            builder.Entity<CategoryBudget>()
                .HasIndex(b => new { b.UserId, b.Year, b.Month, b.Category })
                .IsUnique();
        }

        public DbSet<SpendiTrackWeb.Models.Expense> Expense { get; set; } = default!;

        public DbSet<UserBudget> UserBudgets { get; set; } = default!;

        public DbSet<CategoryBudget> CategoryBudgets { get; set; } = default!;

        public DbSet<UserTrackerProfile> UserTrackerProfiles { get; set; } = default!;
    }
}
