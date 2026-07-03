using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Services
{
    public class MonthlyBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly TrackerAccessService _trackerAccessService;

        public MonthlyBudgetService(
            ApplicationDbContext context,
            TrackerAccessService trackerAccessService)
        {
            _context = context;
            _trackerAccessService = trackerAccessService;
        }

        public async Task<UserBudget?> GetBudgetAsync(
            string userId,
            TrackerPeriod period,
            bool seedIfMissing = true)
        {
            var budget = await FindBudgetAsync(userId, period);
            if (budget != null || !seedIfMissing)
                return budget;

            var start = await _trackerAccessService.GetStartPeriodAsync(userId);
            var startKey = start.Year * 100 + start.Month;
            var prior = await FindPriorBudgetAsync(userId, period, startKey);
            if (prior == null)
                return null;

            budget = new UserBudget
            {
                UserId = userId,
                Year = period.Year,
                Month = period.Month,
                MonthlyIncome = prior.MonthlyIncome,
                SavingsPercent = prior.SavingsPercent,
                FixedMonthlyCosts = prior.FixedMonthlyCosts,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserBudgets.Add(budget);
            await _context.SaveChangesAsync();

            await SeedCategoryBudgetsFromPriorAsync(
                userId,
                period,
                new TrackerPeriod { Year = prior.Year, Month = prior.Month });

            return budget;
        }

        public async Task<List<CategoryBudget>> GetCategoryBudgetsAsync(
            string userId,
            TrackerPeriod period,
            bool seedIfMissing = true)
        {
            if (seedIfMissing)
                await GetBudgetAsync(userId, period);

            return await _context.CategoryBudgets
                .Where(b => b.UserId == userId && b.Year == period.Year && b.Month == period.Month)
                .ToListAsync();
        }

        public async Task<UserBudget> SaveUserBudgetAsync(
            string userId,
            TrackerPeriod period,
            BudgetSetupViewModel input)
        {
            var existing = await FindBudgetAsync(userId, period);

            if (existing == null)
            {
                existing = new UserBudget
                {
                    UserId = userId,
                    Year = period.Year,
                    Month = period.Month
                };
                _context.UserBudgets.Add(existing);
            }

            existing.MonthlyIncome = input.MonthlyIncome;
            existing.SavingsPercent = input.SavingsPercent;
            existing.FixedMonthlyCosts = input.FixedMonthlyCosts;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task SaveCategoryBudgetsAsync(
            string userId,
            TrackerPeriod period,
            IReadOnlyList<CategoryAllocationInput> categories)
        {
            foreach (var item in categories)
            {
                var existing = await _context.CategoryBudgets
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId
                        && b.Year == period.Year
                        && b.Month == period.Month
                        && b.Category == item.Category);

                if (existing == null)
                {
                    _context.CategoryBudgets.Add(new CategoryBudget
                    {
                        UserId = userId,
                        Year = period.Year,
                        Month = period.Month,
                        Category = item.Category,
                        AllocatedAmount = item.AllocatedAmount
                    });
                }
                else
                {
                    existing.AllocatedAmount = item.AllocatedAmount;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<UserBudget?> FindBudgetAsync(string userId, TrackerPeriod period) =>
            await _context.UserBudgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId
                    && b.Year == period.Year
                    && b.Month == period.Month);

        private async Task<UserBudget?> FindPriorBudgetAsync(
            string userId,
            TrackerPeriod period,
            int startPeriodKey)
        {
            var periodKey = period.Year * 100 + period.Month;

            return await _context.UserBudgets
                .Where(b =>
                    b.UserId == userId
                    && (b.Year * 100 + b.Month) < periodKey
                    && (b.Year * 100 + b.Month) >= startPeriodKey)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .FirstOrDefaultAsync();
        }

        private async Task SeedCategoryBudgetsFromPriorAsync(
            string userId,
            TrackerPeriod period,
            TrackerPeriod priorPeriod)
        {
            var alreadyExists = await _context.CategoryBudgets
                .AnyAsync(b => b.UserId == userId && b.Year == period.Year && b.Month == period.Month);

            if (alreadyExists)
                return;

            var priorCategories = await _context.CategoryBudgets
                .Where(b =>
                    b.UserId == userId
                    && b.Year == priorPeriod.Year
                    && b.Month == priorPeriod.Month)
                .ToListAsync();

            if (priorCategories.Count == 0)
                return;

            foreach (var cat in priorCategories)
            {
                _context.CategoryBudgets.Add(new CategoryBudget
                {
                    UserId = userId,
                    Year = period.Year,
                    Month = period.Month,
                    Category = cat.Category,
                    AllocatedAmount = cat.AllocatedAmount
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
