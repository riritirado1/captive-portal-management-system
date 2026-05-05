using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace CaptivePortal.Web.Pages.Admin
{
    [Authorize]
    public class BulkUploadModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BulkUploadModel> _logger;

        public BulkUploadModel(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<BulkUploadModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public List<Campaign> Campaigns { get; set; } = new();

        public async Task OnGetAsync()
        {
            Campaigns = await _context.Campaigns
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(BulkUploadInputModel model)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();

            try
            {
                foreach (var file in model.ImageFiles)
                {
                    try
                    {
                        // Validate each file
                        if (!ValidateImageFile(file, out string validationError))
                        {
                            errors.Add($"{file.FileName}: {validationError}");
                            errorCount++;
                            continue;
                        }

                        // Save image
                        string imagePath = await SaveImageAsync(file);

                        // Extract title from filename (remove extension)
                        string title = Path.GetFileNameWithoutExtension(file.FileName);

                        var advertisement = new Advertisement
                        {
                            Title = title,
                            Description = model.DefaultDescription,
                            ImagePath = imagePath,
                            TargetUrl = model.DefaultTargetUrl,
                            Location = model.DefaultLocation,
                            IsActive = model.DefaultIsActive,
                            StartDate = model.DefaultStartDate,
                            EndDate = model.DefaultEndDate,
                            CampaignId = model.DefaultCampaignId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = User.Identity?.Name ?? "System",
                            Status = "Approved", // Auto-approve bulk uploads
                            Priority = model.DefaultPriority,
                            Tags = model.DefaultTags
                        };

                        _context.Advertisements.Add(advertisement);
                        successCount++;

                        _logger.LogInformation("Bulk upload: Created advertisement '{Title}' from file '{FileName}'", 
                            title, file.FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file {FileName} in bulk upload", file.FileName);
                        errors.Add($"{file.FileName}: {ex.Message}");
                        errorCount++;
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // Set success/error messages
                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully uploaded {successCount} advertisement(s)!";
                }

                if (errorCount > 0)
                {
                    TempData["ErrorMessage"] = $"Failed to upload {errorCount} file(s). Errors: {string.Join("; ", errors)}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk advertisement upload");
                TempData["ErrorMessage"] = "An error occurred during bulk upload. Please try again.";
            }

            return RedirectToPage("/Admin/Advertisements");
        }

        private bool ValidateImageFile(IFormFile file, out string error)
        {
            error = string.Empty;

            if (file == null || file.Length == 0)
            {
                error = "File is empty";
                return false;
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                error = "Invalid image format. Supported: JPG, PNG, GIF, WebP";
                return false;
            }

            // Validate file size (max 5MB for bulk upload)
            if (file.Length > 5 * 1024 * 1024)
            {
                error = "File size exceeds 5MB limit";
                return false;
            }

            return true;
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Generate unique filename
            var fileName = $"ad_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "ads");
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/ads/{fileName}";
        }
    }

    public class BulkUploadInputModel
    {
        [Required(ErrorMessage = "Please select at least one image file")]
        public IFormFile[] ImageFiles { get; set; } = Array.Empty<IFormFile>();

        [StringLength(1000)]
        public string? DefaultDescription { get; set; }

        [Url]
        [StringLength(500)]
        public string? DefaultTargetUrl { get; set; }

        [StringLength(100)]
        public string? DefaultLocation { get; set; }

        public bool DefaultIsActive { get; set; } = true;

        public DateTime? DefaultStartDate { get; set; }

        public DateTime? DefaultEndDate { get; set; }

        public int? DefaultCampaignId { get; set; }

        [Range(0, 100)]
        public int DefaultPriority { get; set; } = 0;

        [StringLength(500)]
        public string? DefaultTags { get; set; }
    }
}