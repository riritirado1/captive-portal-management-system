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
    public class AdvertisementsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdvertisementsModel> _logger;

        public AdvertisementsModel(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<AdvertisementsModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public List<Advertisement> Advertisements { get; set; } = new();

        public async Task OnGetAsync()
        {
            Advertisements = await _context.Advertisements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(AddAdvertisementModel model)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload advertisements
                return Page();
            }

            try
            {
                // Handle file upload
                string imagePath = await SaveImageAsync(model.ImageFile);

                var advertisement = new Advertisement
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = imagePath,
                    TargetUrl = model.TargetUrl,
                    Location = model.Location,
                    IsActive = model.IsActive,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Advertisement '{Title}' created successfully", model.Title);
                TempData["SuccessMessage"] = "Advertisement created successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating advertisement");
                TempData["ErrorMessage"] = "Error creating advertisement. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(EditAdvertisementModel model)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload advertisements
                return Page();
            }

            try
            {
                var advertisement = await _context.Advertisements.FindAsync(model.Id);
                if (advertisement == null)
                {
                    TempData["ErrorMessage"] = "Advertisement not found.";
                    return RedirectToPage();
                }

                // Update fields
                advertisement.Title = model.Title;
                advertisement.Description = model.Description;
                advertisement.TargetUrl = model.TargetUrl;
                advertisement.Location = model.Location;
                advertisement.IsActive = model.IsActive;
                advertisement.StartDate = model.StartDate;
                advertisement.EndDate = model.EndDate;

                // Handle new image upload
                if (model.ImageFile != null)
                {
                    // Delete old image
                    DeleteImage(advertisement.ImagePath);
                    
                    // Save new image
                    advertisement.ImagePath = await SaveImageAsync(model.ImageFile);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Advertisement '{Title}' updated successfully", model.Title);
                TempData["SuccessMessage"] = "Advertisement updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement {Id}", model.Id);
                TempData["ErrorMessage"] = "Error updating advertisement. Please try again.";
            }

            return RedirectToPage();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Image file is required");

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                throw new ArgumentException("Invalid image format. Supported formats: JPG, PNG, GIF, WebP");

            // Validate file size (max 2MB)
            if (imageFile.Length > 2 * 1024 * 1024)
                throw new ArgumentException("Image file size must be less than 2MB");

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

        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting image file: {ImagePath}", imagePath);
            }
        }
    }

    public class AddAdvertisementModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile ImageFile { get; set; } = null!;

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string? TargetUrl { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class EditAdvertisementModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string? TargetUrl { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}