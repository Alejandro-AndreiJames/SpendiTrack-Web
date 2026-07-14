using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Models;
using SpendiTrackWeb.Services;

namespace SpendiTrackWeb.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BudgetCalculator _budgetCalculator;
        private readonly MonthlyBudgetService _monthlyBudgetService;
        private readonly TrackerAccessService _trackerAccessService;
        private readonly UserManager<IdentityUser> _userManager;

        public ExpensesController(
            ApplicationDbContext context,
            BudgetCalculator budgetCalculator,
            MonthlyBudgetService monthlyBudgetService,
            TrackerAccessService trackerAccessService,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _budgetCalculator = budgetCalculator;
            _monthlyBudgetService = monthlyBudgetService;
            _trackerAccessService = trackerAccessService;
            _userManager = userManager;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(int? year, int? month, string? search, string? category)
        {
            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            ApplyTransactionFilters(model, search, category);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBudget(BudgetSetupViewModel input)
        {
            var period = await ResolveTrackerPeriodAsync(input.Year, input.Month);

            if (!ModelState.IsValid)
            {
                var expenses = await GetUserExpensesForPeriodAsync(period);
                var model = BuildIndexViewModel(expenses);
                model.HasBudgetSetup = false;
                model.MonthlyIncome = input.MonthlyIncome;
                model.SavingsPercent = input.SavingsPercent;
                model.FixedMonthlyCosts = input.FixedMonthlyCosts;

                await ApplyCategoryBudgetsToModelAsync(model, expenses, period);
                ApplyBudgetBreakdownToModel(model);
                await ApplyPeriodToModelAsync(model, period);
                return View("Index", model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge();
            }

            await _monthlyBudgetService.SaveUserBudgetAsync(user.Id, period, input);
            return RedirectToTracker(period);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategoryBudgets(CategoryBudgetSetupViewModel input)
        {
            var period = await ResolveTrackerPeriodAsync(input.Year, input.Month);

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Challenge();

            var userBudget = await _monthlyBudgetService.GetBudgetAsync(user.Id, period, seedIfMissing: false);

            if (userBudget == null)
                return RedirectToTracker(period);

            if (input.Categories == null || input.Categories.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No category budget data was submitted.");
                var errorModel = await LoadIndexViewModelAsync(period);
                return View("Index", errorModel);
            }

            var limit = _budgetCalculator.Calculate(userBudget, 0).SpendingLimit;
            var totalAllocated = input.Categories.Sum(c => c.AllocatedAmount);

            if (totalAllocated > limit)
            {
                ModelState.AddModelError(string.Empty,
                    $"Total allocations ({totalAllocated:C}) cannot exceed your spending limit ({limit:C}).");

                var model = await LoadIndexViewModelAsync(period);
                ApplySubmittedCategoryForm(model, input.Categories);
                TempData["OpenCategoryBudgetEdit"] = true;

                return View("Index", model);
            }

            await _monthlyBudgetService.SaveCategoryBudgetsAsync(user.Id, period, input.Categories);
            return RedirectToTracker(period);
        }

        // GET: Search — legacy route (redirect to Tracker)
        public IActionResult ShowSearchForm()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Transaction history search (partial update, no full page reload)
        [HttpGet]
        public async Task<IActionResult> SearchTransactions(int? year, int? month, string? search, string? category)
        {
            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            ApplyTransactionFilters(model, search, category);
            return PartialView("_TransactionHistoryPanel", model);
        }

        // GET: Category utilization refresh (after expense changes)
        [HttpGet]
        public async Task<IActionResult> RefreshCategoryUtilization(int? year, int? month)
        {
            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            return PartialView("_CategoryUtilization", model);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id, int? year, int? month)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await FindUserExpenseAsync(id.Value);
            if (expense == null)
            {
                return NotFound();
            }

            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            model.ViewExpense = expense;
            model.OpenViewExpenseModal = true;
            return View("Index", model);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,Category")] Expense expense)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge();
            }

            expense.UserId = user.Id;
            ModelState.Remove(nameof(Expense.UserId));

            if (ModelState.IsValid)
            {
                if (!await TryValidateCategoryBudgetAsync(user.Id, expense))
                    return await IndexWithDraftAsync(expense);

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToTracker(await ResolveTrackerPeriodAsync(TrackerPeriod.FromDate(expense.Date)));
            }

            return await IndexWithDraftAsync(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id, int? year, int? month)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await FindUserExpenseAsync(id.Value);
            if (expense == null)
            {
                return NotFound();
            }

            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            model.EditExpense = expense;
            model.OpenEditExpenseModal = true;
            return View("Index", model);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Category")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge();
            }

            var existing = await FindUserExpenseAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Expense.UserId));

            if (ModelState.IsValid)
            {
                if (!await TryValidateCategoryBudgetAsync(user.Id, expense, existing))
                {
                    return await IndexWithEditDraftAsync(
                        expense,
                        await ResolveTrackerPeriodAsync(TrackerPeriod.FromDate(expense.Date)));
                }

                try
                {
                    existing.Description = expense.Description;
                    existing.Amount = expense.Amount;
                    existing.Date = expense.Date;
                    existing.Category = expense.Category;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExpenseExistsAsync(expense.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToTracker(await ResolveTrackerPeriodAsync(TrackerPeriod.FromDate(expense.Date)));
            }

            return await IndexWithEditDraftAsync(
                expense,
                await ResolveTrackerPeriodAsync(TrackerPeriod.FromDate(expense.Date)));
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id, int? year, int? month)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await FindUserExpenseAsync(id.Value);
            if (expense == null)
            {
                return NotFound();
            }

            var period = await ResolveTrackerPeriodAsync(year, month);
            var model = await LoadIndexViewModelAsync(period);
            model.DeleteExpense = expense;
            model.OpenDeleteExpenseModal = true;
            return View("Index", model);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? year, int? month)
        {
            var period = await ResolveTrackerPeriodAsync(year, month);

            var expense = await FindUserExpenseAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
                await _context.SaveChangesAsync();
            }

            if (IsAjaxRequest(Request))
            {
                var model = await LoadIndexViewModelAsync(period);
                return Json(new
                {
                    success = true,
                    stats = new
                    {
                        model.HasBudgetSetup,
                        model.MonthlyTotal,
                        model.RemainingBudget,
                        model.TotalAmount,
                        model.TransactionCount,
                        model.AverageAmount
                    }
                });
            }

            return RedirectToTracker(period);
        }

        private static bool IsAjaxRequest(HttpRequest request) =>
            string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);

        private RedirectToActionResult RedirectToTracker(TrackerPeriod period, string? search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction(nameof(Index), new
                {
                    year = period.Year,
                    month = period.Month,
                    search
                });
            }

            return RedirectToAction(nameof(Index), new { year = period.Year, month = period.Month });
        }

        private async Task<IdentityUser?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private async Task<IQueryable<Expense>> GetUserExpensesQueryAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return _context.Expense.Where(_ => false);
            }

            return _context.Expense.Where(e => e.UserId == user.Id);
        }

        private async Task<List<Expense>> GetUserExpensesForPeriodAsync(TrackerPeriod period)
        {
            return await (await GetUserExpensesQueryAsync())
                .Where(e => e.Date >= period.MonthStart && e.Date < period.MonthEnd)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        private async Task<Expense?> FindUserExpenseAsync(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return null;
            }

            return await _context.Expense
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == user.Id);
        }

        private async Task<bool> UserExpenseExistsAsync(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return false;
            }

            return await _context.Expense.AnyAsync(e => e.Id == id && e.UserId == user.Id);
        }

        private async Task ApplyBudgetToModelAsync(ExpenseIndexViewModel model, TrackerPeriod period)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                model.HasBudgetSetup = false;
                return;
            }

            var budget = await _monthlyBudgetService.GetBudgetAsync(user.Id, period);

            if (budget == null)
            {
                model.HasBudgetSetup = false;
                return;
            }

            var result = _budgetCalculator.Calculate(budget, model.MonthlyTotal);
            _budgetCalculator.ApplyToViewModel(result, model);
        }

        private async Task ApplyCategoryBudgetsToModelAsync(
            ExpenseIndexViewModel model,
            IReadOnlyList<Expense> expenses,
            TrackerPeriod period)
        {
            if (!model.HasBudgetSetup)
                return;

            var user = await GetCurrentUserAsync();
            if (user == null)
                return;

            var budgets = await _monthlyBudgetService.GetCategoryBudgetsAsync(user.Id, period);

            var spentByCategory = expenses
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            model.CategoryBudgets = _budgetCalculator.BuildCategorySummaries(
                ExpenseCategories.All, budgets, spentByCategory);

            model.ActiveCategoryBudgets = model.CategoryBudgets
                .Where(c => c.Allocated > 0)
                .OrderByDescending(c => (int)c.AlertLevel)
                .ThenByDescending(c => c.SpentPercentOfAllocated)
                .ThenByDescending(c => c.Allocated)
                .ToList();

            model.CategoryBudgetWarnings = model.ActiveCategoryBudgets
                .Where(c => c.AlertLevel != CategoryUtilizationAlert.None)
                .ToList();

            model.TotalAllocated = _budgetCalculator.TotalAllocated(budgets);
            model.UnallocatedFromLimit = _budgetCalculator.Unallocated(
                model.SpendingLimit, model.TotalAllocated);

            model.HasCategoryBudgetSetup = budgets.Count > 0;

            model.CategoryAllocationForm = ExpenseCategories.All
                .Select(cat => new CategoryAllocationInput
                {
                    Category = cat,
                    AllocatedAmount = budgets.FirstOrDefault(b => b.Category == cat)?.AllocatedAmount ?? 0
                })
                .ToList();
        }

        private void ApplyBudgetBreakdownToModel(ExpenseIndexViewModel model)
        {
            if (!model.HasBudgetSetup)
                return;

            var income = new BudgetCalculationResult
            {
                MonthlyIncome = model.MonthlyIncome,
                SavingsPercent = model.SavingsPercent,
                SavingsAmount = model.SavingsAmount,
                FixedMonthlyCosts = model.FixedMonthlyCosts,
                SpendingLimit = model.SpendingLimit,
                RemainingBudget = model.RemainingBudget,
            };

            model.BudgetBreakdown = _budgetCalculator.BuildBreakdown(
                income,
                model.TotalAllocated,
                model.MonthlyTotal);
        }

        private async Task ApplyPeriodToModelAsync(ExpenseIndexViewModel model, TrackerPeriod period)
        {
            model.SelectedYear = period.Year;
            model.SelectedMonth = period.Month;
            model.IsCurrentMonth = period.IsCurrentMonth;
            model.SelectedMonthLabel = period.DisplayLabel;

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var start = await _trackerAccessService.GetStartPeriodAsync(user.Id);
                model.AvailableMonths = GetAvailableMonths(start, period);
            }
            else
            {
                model.AvailableMonths =
                [
                    new TrackerMonthOption
                    {
                        Year = period.Year,
                        Month = period.Month,
                        Label = period.DisplayLabel,
                        IsSelected = true
                    }
                ];
            }

            var previous = period.AddMonths(-1);
            var next = period.AddMonths(1);
            var startPeriod = user != null
                ? await _trackerAccessService.GetStartPeriodAsync(user.Id)
                : period;

            model.PreviousYear = previous.Year;
            model.PreviousMonth = previous.Month;
            model.NextYear = next.Year;
            model.NextMonth = next.Month;
            model.HasPreviousMonth = previous.MonthStart >= startPeriod.MonthStart;
            model.HasNextMonth = !period.IsCurrentMonth;
        }

        private static List<TrackerMonthOption> GetAvailableMonths(
            TrackerPeriod start,
            TrackerPeriod selected)
        {
            var current = TrackerPeriod.Current();
            var options = new List<TrackerMonthOption>();
            var cursor = current;

            while (cursor.MonthStart >= start.MonthStart)
            {
                options.Add(new TrackerMonthOption
                {
                    Year = cursor.Year,
                    Month = cursor.Month,
                    Label = cursor.DisplayLabel,
                    IsSelected = cursor.Year == selected.Year && cursor.Month == selected.Month
                });
                cursor = cursor.AddMonths(-1);
            }

            return options;
        }

        private async Task<TrackerPeriod> ResolveTrackerPeriodAsync(int? year, int? month)
        {
            var user = await GetCurrentUserAsync();
            var start = user != null
                ? await _trackerAccessService.GetStartPeriodAsync(user.Id)
                : null;

            return TrackerPeriod.Resolve(year, month, start);
        }

        private async Task<TrackerPeriod> ResolveTrackerPeriodAsync(TrackerPeriod period)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return TrackerPeriod.Resolve(period.Year, period.Month);

            var start = await _trackerAccessService.GetStartPeriodAsync(user.Id);
            return _trackerAccessService.ClampPeriod(period, start);
        }

        private async Task<ExpenseIndexViewModel> LoadIndexViewModelAsync(
            TrackerPeriod period,
            List<Expense>? periodExpenses = null)
        {
            periodExpenses ??= await GetUserExpensesForPeriodAsync(period);
            var model = BuildIndexViewModel(periodExpenses);

            await ApplyBudgetToModelAsync(model, period);
            await ApplyCategoryBudgetsToModelAsync(model, periodExpenses, period);
            ApplyBudgetBreakdownToModel(model);
            await ApplyPeriodToModelAsync(model, period);

            return model;
        }

        private static void ApplyTransactionFilters(ExpenseIndexViewModel model, string? search, string? category)
        {
            IEnumerable<Expense> filtered = model.Expenses;

            var trimmedSearch = search?.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedSearch))
            {
                filtered = filtered.Where(e =>
                    e.Description.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase));
                model.SearchPhrase = trimmedSearch;
            }

            var trimmedCategory = category?.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedCategory)
                && ExpenseCategories.All.Contains(trimmedCategory, StringComparer.Ordinal))
            {
                filtered = filtered.Where(e => e.Category == trimmedCategory);
                model.CategoryFilter = trimmedCategory;
            }

            model.Expenses = filtered
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        private async Task<IActionResult> IndexWithEditDraftAsync(Expense expense, TrackerPeriod period)
        {
            var model = await LoadIndexViewModelAsync(period);
            model.EditExpense = expense;
            model.OpenEditExpenseModal = true;

            return View("Index", model);
        }

        private async Task<IActionResult> IndexWithDraftAsync(Expense expense)
        {
            var period = await ResolveTrackerPeriodAsync(null, null);
            var model = await LoadIndexViewModelAsync(period);
            model.DraftExpense = expense;
            model.OpenAddExpenseModal = true;

            return View("Index", model);
        }

        private async Task<bool> TryValidateCategoryBudgetAsync(
            string userId,
            Expense expense,
            Expense? existingExpense = null)
        {
            var period = TrackerPeriod.FromDate(expense.Date);
            var userBudget = await _monthlyBudgetService.GetBudgetAsync(userId, period, seedIfMissing: false);
            if (userBudget == null)
                return true;

            var categoryBudgets = await _monthlyBudgetService.GetCategoryBudgetsAsync(
                userId,
                period,
                seedIfMissing: false);

            var allocated = categoryBudgets
                .FirstOrDefault(b => b.Category == expense.Category)
                ?.AllocatedAmount ?? 0;

            var periodExpenses = await GetUserExpensesForPeriodAsync(period);
            var spentInCategory = periodExpenses
                .Where(e => e.Category == expense.Category)
                .Sum(e => e.Amount);

            var amountToExclude = 0m;
            if (existingExpense != null
                && existingExpense.Category == expense.Category
                && TrackerPeriod.FromDate(existingExpense.Date).Year == period.Year
                && TrackerPeriod.FromDate(existingExpense.Date).Month == period.Month)
            {
                amountToExclude = existingExpense.Amount;
            }

            var result = _budgetCalculator.CheckCategoryExpenseLimit(
                allocated,
                spentInCategory,
                expense.Amount,
                amountToExclude,
                expense.Category,
                period.DisplayLabel);

            if (result.IsAllowed)
                return true;

            ModelState.AddModelError("CategoryBudgetLimit", result.ErrorMessage!);
            return false;
        }

        private static void ApplySubmittedCategoryForm(
            ExpenseIndexViewModel model,
            List<CategoryAllocationInput> submitted)
        {
            if (submitted == null || submitted.Count == 0)
                return;

            model.CategoryAllocationForm = submitted;
        }

        private static ExpenseIndexViewModel BuildIndexViewModel(IReadOnlyList<Expense> periodExpenses)
        {
            return new ExpenseIndexViewModel
            {
                Expenses = periodExpenses,
                TransactionCount = periodExpenses.Count,
                TotalAmount = periodExpenses.Sum(e => e.Amount),
                MonthlyTotal = periodExpenses.Sum(e => e.Amount),
                AverageAmount = periodExpenses.Count > 0 ? periodExpenses.Average(e => e.Amount) : 0,
                LargestExpense = periodExpenses.Count > 0 ? periodExpenses.Max(e => e.Amount) : 0,
                CategoryTotals = periodExpenses
                    .GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Sum(e => e.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount))
            };
        }
    }
}
