using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expense.ToListAsync();
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            ViewBag.TotalSpent = expenses.Sum(e => e.Amount);
            ViewBag.MonthlySpent = expenses.Where(e => e.Date >= monthStart).Sum(e => e.Amount);
            ViewBag.TransactionCount = expenses.Count;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
