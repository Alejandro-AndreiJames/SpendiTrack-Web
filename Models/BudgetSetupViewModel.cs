using System.ComponentModel.DataAnnotations;

namespace SpendiTrackWeb.Models
{
    public class BudgetSetupViewModel
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }

        [Range(0, 100)]
        public decimal SavingsPercent { get; set; } = 10;

        [Range(0, double.MaxValue)]
        public decimal FixedMonthlyCosts { get; set; }
    }
}