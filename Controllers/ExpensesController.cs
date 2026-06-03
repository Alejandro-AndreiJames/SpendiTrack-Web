using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expense
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            return View(BuildIndexViewModel(expenses));
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
            var query = _context.Expense.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                query = query.Where(e => e.Description.Contains(searchPhrase));
            }

            var expenses = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var model = BuildIndexViewModel(expenses);
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

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        [Authorize]
        public IActionResult Create()
        {
            return View(new Expense { Date = DateTime.Today });
        }

        // POST: Expenses/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,Category")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: Expenses/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            return View(expense);
        }

        // POST: Expenses/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,Category")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: Expenses/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expense.Any(e => e.Id == id);
        }

        private static ExpenseIndexViewModel BuildIndexViewModel(IReadOnlyList<Expense> expenses)
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            return new ExpenseIndexViewModel
            {
                Expenses = expenses,
                TransactionCount = expenses.Count,
                TotalAmount = expenses.Sum(e => e.Amount),
                MonthlyTotal = expenses.Where(e => e.Date >= monthStart).Sum(e => e.Amount),
                AverageAmount = expenses.Count > 0 ? expenses.Average(e => e.Amount) : 0,
                LargestExpense = expenses.Count > 0 ? expenses.Max(e => e.Amount) : 0,
                CategoryTotals = expenses
                    .GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Sum(e => e.Amount))
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount))
            };
        }
    }
}
