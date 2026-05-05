using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly SignInManager<IdentityUser> _signInManager;

        public IndexModel(ApplicationDbContext context, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        public int TodayConnections { get; set; }
        public int WeekConnections { get; set; }
        public int TotalEmails { get; set; }
        public int UniqueEmails { get; set; }
        public int ActiveAds { get; set; }
        public int TotalAds { get; set; }
        public double TodayGrowth { get; set; }
        public double WeekGrowth { get; set; }
        public List<PortalUser> RecentConnections { get; set; } = new();

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var lastWeekStart = weekStart.AddDays(-7);

            // Calculate statistics
            TodayConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date == today);

            var yesterdayConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date == yesterday);

            WeekConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date >= weekStart);

            var lastWeekConnections = await _context.PortalUsers
                .CountAsync(u => u.ConnectedAt.Date >= lastWeekStart && u.ConnectedAt.Date < weekStart);

            TotalEmails = await _context.PortalUsers.CountAsync();
            
            UniqueEmails = await _context.PortalUsers
                .Select(u => u.Email)
                .Distinct()
                .CountAsync();

            ActiveAds = await _context.Advertisements
                .CountAsync(a => a.IsActive);

            TotalAds = await _context.Advertisements.CountAsync();

            // Calculate growth percentages
            TodayGrowth = yesterdayConnections > 0 
                ? Math.Round(((double)(TodayConnections - yesterdayConnections) / yesterdayConnections) * 100, 1)
                : 0;

            WeekGrowth = lastWeekConnections > 0
                ? Math.Round(((double)(WeekConnections - lastWeekConnections) / lastWeekConnections) * 100, 1)
                : 0;

            // Get recent connections (last 10)
            RecentConnections = await _context.PortalUsers
                .OrderByDescending(u => u.ConnectedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}