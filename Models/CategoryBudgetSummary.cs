namespace SpendiTrackWeb.Models
{
    public class CategoryBudgetSummary
    {
        public const double ApproachThresholdPercent = 80;

        public string Category { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining => Allocated - Spent;

        public double SpentPercentOfAllocated =>
            Allocated > 0 ? (double)(Spent / Allocated * 100) : 0;

        public bool IsOverLimit => Allocated > 0 && Spent > Allocated;

        public CategoryUtilizationAlert AlertLevel
        {
            get
            {
                if (Allocated <= 0)
                    return CategoryUtilizationAlert.None;

                if (Spent >= Allocated)
                    return CategoryUtilizationAlert.Exceeded;

                if (SpentPercentOfAllocated >= ApproachThresholdPercent)
                    return CategoryUtilizationAlert.Approach;

                return CategoryUtilizationAlert.None;
            }
        }

        public string AlertLabel => AlertLevel switch
        {
            CategoryUtilizationAlert.Exceeded when IsOverLimit => "Over limit",
            CategoryUtilizationAlert.Exceeded => "Limit reached",
            CategoryUtilizationAlert.Approach => "Approaching limit",
            _ => string.Empty
        };
    }
}
