namespace SpendiTrackWeb.Models
{
    public class TrackerMonthOption
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsSelected { get; set; }

        public int SortKey => Year * 100 + Month;
    }
}
