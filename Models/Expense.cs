using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendiTrackWeb.Models
{
    public class Expense
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Add a description.")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be at least ₱0.01.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "Pick a category.")]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public Expense()
        {

        }

    }
}
