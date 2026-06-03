namespace SpendiTrackWeb.Models
{
    public class ExpenseIndexViewModel
    {
        public IEnumerable<Expense> Expenses { get; set; } = [];
        public decimal TotalAmount { get; set; }
        public decimal MonthlyTotal { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal LargestExpense { get; set; }
        public IReadOnlyDictionary<string, decimal> CategoryTotals { get; set; } =
            new Dictionary<string, decimal>();
        public string? SearchPhrase { get; set; }
    }
}
