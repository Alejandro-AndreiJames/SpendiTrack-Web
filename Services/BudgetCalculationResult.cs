namespace SpendiTrackWeb.Services
{
    /// <summary>
    /// Holds the numbers after we run budget formulas.
    /// Not stored in DB — built each time we load Tracker.
    /// </summary>
    public class BudgetCalculationResult
    {
        public decimal MonthlyIncome { get; set; }
        public decimal SavingsPercent { get; set; }
        public decimal FixedMonthlyCosts { get; set; }
        ///Money set aside for savings this month.
        public decimal SavingsAmount { get; set; }
        ///Max you plan to spend this month (income - savings - fixed).
        public decimal SpendingLimit { get; set; }
        ///Limit minus expenses already spent this month.
        public decimal RemainingBudget { get; set; }
    }
}
