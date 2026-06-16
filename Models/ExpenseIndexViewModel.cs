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

        // Income / limit feature
        public bool HasBudgetSetup { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal SavingsPercent { get; set; }
        public decimal SavingsAmount { get; set; }
        public decimal FixedMonthlyCosts { get; set; }
        public decimal SpendingLimit { get; set; }
        public decimal RemainingBudget { get; set; }

        // For Budget Allocation feature
        public bool HasCategoryBudgetSetup { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal UnallocatedFromLimit { get; set; }
        public List<CategoryAllocationInput> CategoryAllocationForm { get; set; } = new();
        public List<CategoryBudgetSummary> CategoryBudgets { get; set; } = new();
        public List<CategoryBudgetSummary> ActiveCategoryBudgets { get; set; } = new();
        public bool HasActiveCategoryAllocations => ActiveCategoryBudgets.Count > 0;

        // R3 — budget breakdown
        public List<BudgetBreakdownLine> BudgetBreakdown { get; set; } = new();
    }
}
