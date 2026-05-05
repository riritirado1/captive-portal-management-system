using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Services;

namespace CaptivePortal.Web.Pages;

public class SuccessModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWiFiAccessService _wifiAccessService;

    public SuccessModel(ApplicationDbContext context, IWiFiAccessService wifiAccessService)
    {
        _context = context;
        _wifiAccessService = wifiAccessService;
    }

    public string Email { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime? AccessExpiresAt { get; set; }
    public string? RedirectUrl { get; set; }
    public string NetworkName { get; set; } = "WTAMU-WiFi";
    public bool IsReconnection { get; set; }
    public int ReconnectionCount { get; set; }
    public bool AutoRedirectEnabled { get; set; } = true;
    public int RedirectDelay { get; set; } = 5;

    public async Task OnGetAsync(string? sessionId, string? email, string? location, string? redirectUrl)
    {
        SessionId = sessionId ?? string.Empty;
        Email = email ?? "Connected User";
        Location = location ?? "Campus";
        RedirectUrl = redirectUrl;

        // Get user details from database if session ID is provided
        if (!string.IsNullOrEmpty(sessionId))
        {
            var user = await _context.PortalUsers
                .FirstOrDefaultAsync(u => u.SessionId == sessionId);

            if (user != null)
            {
                Email = user.Email;
                ConnectedAt = user.AccessGrantedAt ?? user.ConnectedAt;
                Location = user.Location ?? "Campus";
                AccessExpiresAt = user.AccessExpiresAt;
                IsReconnection = user.ReconnectionCount > 1;
                ReconnectionCount = user.ReconnectionCount;
                RedirectUrl = user.RedirectUrl ?? redirectUrl;
            }
        }

        // Set defaults if not found
        if (ConnectedAt == default)
            ConnectedAt = DateTime.Now;
    }
}