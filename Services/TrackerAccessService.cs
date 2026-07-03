using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Services
{
    public class TrackerAccessService
    {
        private readonly ApplicationDbContext _context;

        public TrackerAccessService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrackerPeriod> GetStartPeriodAsync(string userId)
        {
            var profile = await _context.UserTrackerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                var now = DateTime.Now;
                return new TrackerPeriod { Year = now.Year, Month = now.Month };
            }

            return new TrackerPeriod { Year = profile.StartYear, Month = profile.StartMonth };
        }

        public async Task CreateProfileForNewUserAsync(string userId, DateTime registeredAt)
        {
            var exists = await _context.UserTrackerProfiles
                .AnyAsync(p => p.UserId == userId);

            if (exists)
                return;

            _context.UserTrackerProfiles.Add(new UserTrackerProfile
            {
                UserId = userId,
                StartYear = registeredAt.Year,
                StartMonth = registeredAt.Month
            });

            await _context.SaveChangesAsync();
        }

        public TrackerPeriod ClampPeriod(TrackerPeriod period, TrackerPeriod startPeriod)
        {
            var current = TrackerPeriod.Current();

            if (period.MonthStart > current.MonthStart)
                period = current;

            if (period.MonthStart < startPeriod.MonthStart)
                period = startPeriod;

            return period;
        }
    }
}
