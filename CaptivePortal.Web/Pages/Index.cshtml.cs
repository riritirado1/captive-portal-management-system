using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;

namespace CaptivePortal.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
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
            // Create portal user record
            var portalUser = new PortalUser
            {
                Email = Email.Trim().ToLowerInvariant(),
                ConnectedAt = DateTime.UtcNow,
                AcceptedTerms = AcceptTerms,
                TermsAcceptedAt = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                Location = GetConnectionLocation()
            };

            // Check if user already exists (within last hour)
            var existingUser = await _context.PortalUsers
                .FirstOrDefaultAsync(u => u.Email == portalUser.Email && 
                                        u.ConnectedAt > DateTime.UtcNow.AddHours(-1));

            if (existingUser == null)
            {
                _context.PortalUsers.Add(portalUser);
                await _context.SaveChangesAsync();
            }

            // Log successful connection
            _logger.LogInformation("User {Email} connected from {IpAddress} at {Location}", 
                Email, portalUser.IpAddress, portalUser.Location);

            // Redirect to success page with original URL
            var returnUrl = Request.Query["returnUrl"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing portal connection for {Email}", Email);
            ModelState.AddModelError("", "An error occurred while connecting. Please try again.");
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
}
