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
        public string? CategoryFilter { get; set; }
        public bool HasTransactionFilters =>
            !string.IsNullOrWhiteSpace(SearchPhrase) || !string.IsNullOrWhiteSpace(CategoryFilter);

        // Selected tracker month
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public bool IsCurrentMonth { get; set; }
        public string SelectedMonthLabel { get; set; } = string.Empty;
        public List<TrackerMonthOption> AvailableMonths { get; set; } = new();
        public bool HasPreviousMonth { get; set; }
        public int PreviousYear { get; set; }
        public int PreviousMonth { get; set; }
        public bool HasNextMonth { get; set; }
        public int NextYear { get; set; }
        public int NextMonth { get; set; }

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

        // Budget breakdown
        public List<BudgetBreakdownLine> BudgetBreakdown { get; set; } = new();

        // Add expense modal
        public Expense? DraftExpense { get; set; }
        public bool OpenAddExpenseModal { get; set; }

        // View / edit expense modals
        public Expense? ViewExpense { get; set; }
        public bool OpenViewExpenseModal { get; set; }
        public Expense? EditExpense { get; set; }
        public bool OpenEditExpenseModal { get; set; }

        // Delete expense modal
        public Expense? DeleteExpense { get; set; }
        public bool OpenDeleteExpenseModal { get; set; }
    }
}
