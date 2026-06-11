using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendiTrackWeb.Models
{
    public class Expense
    {

        public int Id { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public Expense()
        {

        }

    }
}
