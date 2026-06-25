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
        private readonly UserManager<IdentityUser> _userManager;

        public ExpensesController(
            ApplicationDbContext context,
            BudgetCalculator budgetCalculator,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _budgetCalculator = budgetCalculator;
            _userManager = userManager;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var model = await LoadIndexViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBudget(BudgetSetupViewModel input)
        {
            if (!ModelState.IsValid)
            {
                var expenses = await GetUserExpensesOrderedAsync();
                var model = BuildIndexViewModel(expenses);
                model.HasBudgetSetup = false;
                model.MonthlyIncome = input.MonthlyIncome;
                model.SavingsPercent = input.SavingsPercent;
                model.FixedMonthlyCosts = input.FixedMonthlyCosts;

                await ApplyCategoryBudgetsToModelAsync(model, expenses);
                ApplyBudgetBreakdownToModel(model);
                return View("Index", model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Challenge();
            }

            var existing = await _context.UserBudgets
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (existing == null)
            {
                _context.UserBudgets.Add(new UserBudget
                {
                    UserId = user.Id,
                    MonthlyIncome = input.MonthlyIncome,
                    SavingsPercent = input.SavingsPercent,
                    FixedMonthlyCosts = input.FixedMonthlyCosts,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.MonthlyIncome = input.MonthlyIncome;
                existing.SavingsPercent = input.SavingsPercent;
                existing.FixedMonthlyCosts = input.FixedMonthlyCosts;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategoryBudgets(CategoryBudgetSetupViewModel input)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Challenge();

            var userBudget = await _context.UserBudgets
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (userBudget == null)
                return RedirectToAction(nameof(Index));

            if (input.Categories == null || input.Categories.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No category budget data was submitted.");
                var errorModel = await LoadIndexViewModelAsync();
                return View("Index", errorModel);
            }

            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthlySpent = await _context.Expense
                .Where(e => e.UserId == user.Id && e.Date >= monthStart)
                .SumAsync(e => e.Amount);

            var limit = _budgetCalculator.Calculate(userBudget, monthlySpent).SpendingLimit;
            var totalAllocated = input.Categories.Sum(c => c.AllocatedAmount);

            if (totalAllocated > limit)
            {
                ModelState.AddModelError(string.Empty,
                    $"Total allocations ({totalAllocated:C}) cannot exceed your spending limit ({limit:C}).");

                var model = await LoadIndexViewModelAsync();
                ApplySubmittedCategoryForm(model, input.Categories);
                TempData["OpenCategoryBudgetEdit"] = true;

                return View("Index", model);
            }

            foreach (var item in input.Categories)
            {
                var existing = await _context.CategoryBudgets
                    .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Category == item.Category);

                if (existing == null)
                {
                    _context.CategoryBudgets.Add(new CategoryBudget
                    {
                        UserId = user.Id,
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
            return RedirectToAction(nameof(Index));
        }

        // GET: Search
        public IActionResult ShowSearchForm()
        {
            return View();
        }

        // POST: Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowSearchResults(string? searchPhrase)
        {
            var query = await GetUserExpensesQueryAsync();
            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                query = query.Where(e => e.Description.Contains(searchPhrase));
            }

            var expenses = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var model = await LoadIndexViewModelAsync(expenses);
            model.SearchPhrase = searchPhrase;

            return View("Index", model);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
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

            var model = await LoadIndexViewModelAsync();
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
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return await IndexWithDraftAsync(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

            var model = await LoadIndexViewModelAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return await IndexWithEditDraftAsync(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await FindUserExpenseAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private async Task<List<Expense>> GetUserExpensesOrderedAsync()
        {
            return await (await GetUserExpensesQueryAsync())
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

        private async Task ApplyBudgetToModelAsync(ExpenseIndexViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                model.HasBudgetSetup = false;
                return;
            }

            var budget = await _context.UserBudgets
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

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
            IReadOnlyList<Expense> expenses)
        {
            if (!model.HasBudgetSetup)
                return;

            var user = await GetCurrentUserAsync();
            if (user == null)
                return;

            var budgets = await _context.CategoryBudgets
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var spentByCategory = expenses
                .Where(e => e.Date >= monthStart)
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            model.CategoryBudgets = _budgetCalculator.BuildCategorySummaries(
                ExpenseCategories.All, budgets, spentByCategory);

            model.ActiveCategoryBudgets = model.CategoryBudgets
                .Where(c => c.Allocated > 0)
                .OrderByDescending(c => c.Allocated)
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

        private async Task<ExpenseIndexViewModel> LoadIndexViewModelAsync(List<Expense>? expenses = null)
        {
            expenses ??= await GetUserExpensesOrderedAsync();
            var model = BuildIndexViewModel(expenses);

            await ApplyBudgetToModelAsync(model);
            await ApplyCategoryBudgetsToModelAsync(model, expenses);
            ApplyBudgetBreakdownToModel(model);

            return model;
        }

        private async Task<IActionResult> IndexWithEditDraftAsync(Expense expense)
        {
            var model = await LoadIndexViewModelAsync();
            model.EditExpense = expense;
            model.OpenEditExpenseModal = true;

            return View("Index", model);
        }

        private async Task<IActionResult> IndexWithDraftAsync(Expense expense)
        {
            var model = await LoadIndexViewModelAsync();
            model.DraftExpense = expense;
            model.OpenAddExpenseModal = true;

            return View("Index", model);
        }

        private static void ApplySubmittedCategoryForm(
            ExpenseIndexViewModel model,
            List<CategoryAllocationInput> submitted)
        {
            if (submitted == null || submitted.Count == 0)
                return;

            model.CategoryAllocationForm = submitted;
        }

        private static ExpenseIndexViewModel BuildIndexViewModel(IReadOnlyList<Expense> expenses)
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var expensesThisMonth = expenses.Where(e => e.Date >= monthStart).ToList();

            return new ExpenseIndexViewModel
            {
                Expenses = expenses,
                TransactionCount = expenses.Count,
                TotalAmount = expenses.Sum(e => e.Amount),
                MonthlyTotal = expensesThisMonth.Sum(e => e.Amount),
                AverageAmount = expenses.Count > 0 ? expenses.Average(e => e.Amount) : 0,
                LargestExpense = expenses.Count > 0 ? expenses.Max(e => e.Amount) : 0,
                CategoryTotals = expensesThisMonth
                    .GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Sum(e => e.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount))
            };
        }
    }
}