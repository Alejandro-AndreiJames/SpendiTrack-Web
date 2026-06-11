using System.ComponentModel.DataAnnotations;
namespace SpendiTrackWeb.Models
{
    public class CategoryAllocationInput
    {
        [Required]
        public string Category { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public decimal AllocatedAmount { get; set; }
    }
    public class CategoryBudgetSetupViewModel
    {
        public List<CategoryAllocationInput> Categories { get; set; } = new();
    }
}
