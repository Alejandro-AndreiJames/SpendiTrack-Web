using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendiTrackWeb.Models
{
    public class UserBudget
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 100)]
        public decimal SavingsPercent { get; set; } = 10;

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue)]
        public decimal FixedMonthlyCosts { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int Year { get; set; }

        public int Month { get; set; }
    }
}
