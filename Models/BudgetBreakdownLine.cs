namespace SpendiTrackWeb.Models
{
    public class BudgetBreakdownLine
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Kind { get; set; } = "normal";
        public bool IsDeduction => Kind == "deduction";
    }
}