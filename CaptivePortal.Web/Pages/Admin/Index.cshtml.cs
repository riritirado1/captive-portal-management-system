using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;

namespace CaptivePortal.Web.Pages.Admin
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TodayConnections { get; set; }
        public int WeekConnections { get; set; }
        public int TotalEmails { get; set; }
        public int ActiveAds { get; set; }
        public List<PortalUser> RecentConnections { get; set; } = new();

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            // Calculate statistics
            TodayConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date == today);

            WeekConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date >= weekStart);

            TotalEmails = await _context.PortalUsers
                .Select(u => u.Email)
                .Distinct()
                .CountAsync();

            ActiveAds = await _context.Advertisements
                .CountAsync(a => a.IsActive);

            // Get recent connections (last 10)
            RecentConnections = await _context.PortalUsers
                .OrderByDescending(u => u.ConnectedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}