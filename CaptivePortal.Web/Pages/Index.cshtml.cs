using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;
using CaptivePortal.Web.Services;

namespace CaptivePortal.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;
    private readonly IWiFiAccessService _wifiAccessService;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger, IWiFiAccessService wifiAccessService)
    {
        _context = context;
        _logger = logger;
        _wifiAccessService = wifiAccessService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of service")]
    public bool AcceptTerms { get; set; }

    public Advertisement? CurrentAd { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCurrentAdvertisement();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCurrentAdvertisement();
            return Page();
        }

        try
        {
            var returnUrl = Request.Query["returnUrl"].ToString();
            
            // Create portal user record
            var portalUser = new PortalUser
            {
                Email = Email.Trim().ToLowerInvariant(),
                ConnectedAt = DateTime.UtcNow,
                AcceptedTerms = AcceptTerms,
                TermsAcceptedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                Location = GetConnectionLocation(),
                MacAddress = GetClientMacAddress()
            };

            // Check if user already exists (within last 24 hours)
            var existingUser = await _context.PortalUsers
                .FirstOrDefaultAsync(u => u.Email == portalUser.Email && 
                                        u.ConnectedAt > DateTime.UtcNow.AddHours(-24));

            if (existingUser != null)
            {
                // Update existing user's reconnection info
                existingUser.ReconnectionCount++;
                existingUser.LastActivityAt = DateTime.UtcNow;
                existingUser.IpAddress = portalUser.IpAddress;
                existingUser.UserAgent = portalUser.UserAgent;
                portalUser = existingUser;
            }
            else
            {
                _context.PortalUsers.Add(portalUser);
            }

            // Grant Wi-Fi access through the service
            var accessResult = await _wifiAccessService.GrantAccessAsync(portalUser, returnUrl);

            if (accessResult.Success)
            {
                await _context.SaveChangesAsync();

                // Log successful connection
                _logger.LogInformation("User {Email} granted Wi-Fi access from {IpAddress} at {Location}. Session: {SessionId}", 
                    Email, portalUser.IpAddress, portalUser.Location, accessResult.SessionId);

                // Redirect to success page with access details
                return RedirectToPage("/Success", new { 
                    sessionId = accessResult.SessionId,
                    email = portalUser.Email,
                    location = portalUser.Location,
                    redirectUrl = accessResult.RedirectUrl
                });
            }
            else
            {
                ModelState.AddModelError("", accessResult.Message);
                await LoadCurrentAdvertisement();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Wi-Fi access request for {Email}", Email);
            ModelState.AddModelError("", "An error occurred while granting Wi-Fi access. Please try again.");
            await LoadCurrentAdvertisement();
            return Page();
        }
    }

    private async Task LoadCurrentAdvertisement()
    {
        var location = GetConnectionLocation();
        
        // Get all active advertisements that match the criteria
        var activeAds = await _context.Advertisements
            .Where(a => a.IsActive && 
                       (a.Location == null || a.Location == location) &&
                       (a.StartDate == null || a.StartDate <= DateTime.UtcNow) &&
                       (a.EndDate == null || a.EndDate >= DateTime.UtcNow))
            .ToListAsync(); // Load to memory first
        
        // Randomly select one advertisement
        if (activeAds.Any())
        {
            var random = new Random();
            CurrentAd = activeAds[random.Next(activeAds.Count)];
        }
        else
        {
            CurrentAd = null;
        }
    }

    private string GetClientIpAddress()
    {
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
        
        if (ipAddress != null)
        {
            // Handle IPv4 mapped to IPv6
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }
            return ipAddress.ToString();
        }
        
        return "Unknown";
    }

    private string GetConnectionLocation()
    {
        // In a real implementation, this would be determined by network configuration
        // For now, we'll use a default location
        return "Stadium";
    }

    private string? GetClientMacAddress()
    {
        // Note: MAC address is typically not available through HTTP requests for security reasons
        // In a real captive portal implementation, this would be obtained through:
        // 1. DHCP logs correlation
        // 2. ARP table lookups on network equipment
        // 3. 802.1X authentication data
        // 4. Network controller APIs
        
        // For demonstration purposes, we'll generate a simulated MAC address
        // based on the IP address to maintain some consistency
        var ipHash = GetClientIpAddress().GetHashCode();
        var mac = $"02:{Math.Abs(ipHash % 256):X2}:{Math.Abs((ipHash >> 8) % 256):X2}:" +
                 $"{Math.Abs((ipHash >> 16) % 256):X2}:{Math.Abs((ipHash >> 24) % 256):X2}:FF";
        
        return mac;
    }
}
