namespace SpendiTrackWeb.Models
{
    public sealed class TrackerPeriod
    {
        public int Year { get; init; }
        public int Month { get; init; }

        public DateTime MonthStart => new(Year, Month, 1);

        public DateTime MonthEnd => MonthStart.AddMonths(1);

        public bool IsCurrentMonth
        {
            get
            {
                var now = DateTime.Now;
                return Year == now.Year && Month == now.Month;
            }
        }

        public string DisplayLabel => MonthStart.ToString("MMMM yyyy");

        public static TrackerPeriod Current()
        {
            var now = DateTime.Now;
            return new TrackerPeriod { Year = now.Year, Month = now.Month };
        }

        public static TrackerPeriod Resolve(int? year, int? month, TrackerPeriod? earliestAllowed = null)
        {
            if (year is null or < 1 or > 9999 || month is null or < 1 or > 12)
                return ClampToBounds(Current(), earliestAllowed);

            var resolved = new TrackerPeriod { Year = year.Value, Month = month.Value };
            return ClampToBounds(resolved, earliestAllowed);
        }

        private static TrackerPeriod ClampToBounds(TrackerPeriod period, TrackerPeriod? earliestAllowed)
        {
            var current = Current();

            if (period.MonthStart > current.MonthStart)
                period = current;

            if (earliestAllowed != null && period.MonthStart < earliestAllowed.MonthStart)
                period = earliestAllowed;

            return period;
        }

        public static TrackerPeriod FromDate(DateTime date) =>
            new() { Year = date.Year, Month = date.Month };

        public TrackerPeriod AddMonths(int delta)
        {
            var next = MonthStart.AddMonths(delta);
            return new TrackerPeriod { Year = next.Year, Month = next.Month };
        }
    }
}
