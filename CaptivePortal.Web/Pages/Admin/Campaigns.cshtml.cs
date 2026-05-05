using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;

namespace CaptivePortal.Web.Pages.Admin
{
    [Authorize]
    public class CampaignsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CampaignsModel> _logger;

        public CampaignsModel(ApplicationDbContext context, ILogger<CampaignsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Campaign> Campaigns { get; set; } = new();

        public async Task OnGetAsync()
        {
            Campaigns = await _context.Campaigns
                .Include(c => c.Advertisements)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(AddCampaignModel model)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var campaign = new Campaign
                {
                    Name = model.Name,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Budget = model.Budget,
                    TargetAudience = model.TargetAudience,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Campaigns.Add(campaign);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Campaign '{Name}' created successfully", model.Name);
                TempData["SuccessMessage"] = "Campaign created successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                TempData["ErrorMessage"] = "Error creating campaign. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(EditCampaignModel model)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                var campaign = await _context.Campaigns.FindAsync(model.Id);
                if (campaign == null)
                {
                    TempData["ErrorMessage"] = "Campaign not found.";
                    return RedirectToPage();
                }

                campaign.Name = model.Name;
                campaign.Description = model.Description;
                campaign.StartDate = model.StartDate;
                campaign.EndDate = model.EndDate;
                campaign.Budget = model.Budget;
                campaign.TargetAudience = model.TargetAudience;
                campaign.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Campaign '{Name}' updated successfully", model.Name);
                TempData["SuccessMessage"] = "Campaign updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign {Id}", model.Id);
                TempData["ErrorMessage"] = "Error updating campaign. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .Include(c => c.Advertisements)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaign == null)
                {
                    return NotFound();
                }

                // Check if campaign has active advertisements
                if (campaign.Advertisements?.Any(a => a.IsActive) == true)
                {
                    TempData["ErrorMessage"] = "Cannot delete campaign with active advertisements. Deactivate advertisements first.";
                    return RedirectToPage();
                }

                // Set CampaignId to null for associated advertisements instead of deleting them
                foreach (var ad in campaign.Advertisements ?? Enumerable.Empty<Advertisement>())
                {
                    ad.CampaignId = null;
                }

                _context.Campaigns.Remove(campaign);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Campaign '{Name}' (ID: {Id}) deleted successfully", campaign.Name, campaign.Id);
                TempData["SuccessMessage"] = "Campaign deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign {Id}", id);
                TempData["ErrorMessage"] = "Error deleting campaign. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetCampaignStatsAsync(int id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Advertisements)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null)
            {
                return NotFound();
            }

            var stats = new
            {
                TotalAdvertisements = campaign.Advertisements?.Count ?? 0,
                ActiveAdvertisements = campaign.Advertisements?.Count(a => a.IsActive) ?? 0,
                TotalViews = campaign.Advertisements?.Sum(a => a.ViewCount) ?? 0,
                TotalClicks = campaign.Advertisements?.Sum(a => a.ClickCount) ?? 0,
                AverageClickRate = campaign.Advertisements?.Any() == true 
                    ? campaign.Advertisements.Where(a => a.ViewCount > 0).Average(a => a.ClickThroughRate) 
                    : 0
            };

            return new JsonResult(stats);
        }
    }

    public class AddCampaignModel
    {
        [Required(ErrorMessage = "Campaign name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal? Budget { get; set; }

        [StringLength(500, ErrorMessage = "Target audience cannot exceed 500 characters")]
        public string? TargetAudience { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class EditCampaignModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Campaign name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal? Budget { get; set; }

        [StringLength(500, ErrorMessage = "Target audience cannot exceed 500 characters")]
        public string? TargetAudience { get; set; }

        public bool IsActive { get; set; } = true;
    }
}