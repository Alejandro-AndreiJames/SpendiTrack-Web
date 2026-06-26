namespace SpendiTrackWeb.Models
{
    public class CategoryBudgetSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining => Allocated - Spent;

        public double SpentPercentOfAllocated =>
            Allocated > 0 ? (double)(Spent / Allocated * 100) : 0;
    }
}