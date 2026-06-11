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
        public DbSet<SpendiTrackWeb.Models.Expense> Expense { get; set; } = default!;

        public DbSet<UserBudget> UserBudgets { get; set; } = default!;

        public DbSet<CategoryBudget> CategoryBudgets { get; set; } = default!;
    }
}
