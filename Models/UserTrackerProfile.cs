using System.ComponentModel.DataAnnotations;

namespace SpendiTrackWeb.Models
{
    public class UserTrackerProfile
    {
        [Key]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int StartYear { get; set; }

        public int StartMonth { get; set; }
    }
}
