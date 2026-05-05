using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;

namespace CaptivePortal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AdsController(ApplicationDbContext context, ILogger<AdsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Track advertisement click
        /// </summary>
        [HttpPost("track-click")]
        public async Task<IActionResult> TrackClick([FromBody] TrackClickRequest request)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == request.AdvertisementId);

                if (advertisement == null)
                {
                    return NotFound(new { message = "Advertisement not found" });
                }

                // Increment click count
                advertisement.ClickCount++;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Advertisement {Id} clicked. Total clicks: {ClickCount}", 
                    advertisement.Id, advertisement.ClickCount);

                return Ok(new { 
                    success = true, 
                    clickCount = advertisement.ClickCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking advertisement click for ID: {AdvertisementId}", request.AdvertisementId);
                return StatusCode(500, new { message = "Error tracking click" });
            }
        }

        /// <summary>
        /// Get advertisement by ID (for editing)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAdvertisement(int id)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                {
                    return NotFound(new { message = "Advertisement not found" });
                }

                return Ok(new
                {
                    id = advertisement.Id,
                    title = advertisement.Title,
                    description = advertisement.Description,
                    imagePath = advertisement.ImagePath,
                    targetUrl = advertisement.TargetUrl,
                    location = advertisement.Location,
                    isActive = advertisement.IsActive,
                    startDate = advertisement.StartDate,
                    endDate = advertisement.EndDate,
                    clickCount = advertisement.ClickCount,
                    createdAt = advertisement.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement {Id}", id);
                return StatusCode(500, new { message = "Error retrieving advertisement" });
            }
        }

        /// <summary>
        /// Get all active advertisements (public endpoint)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAdvertisements([FromQuery] string? location = null)
        {
            try
            {
                var query = _context.Advertisements
                    .Where(a => a.IsActive &&
                               (a.StartDate == null || a.StartDate <= DateTime.UtcNow) &&
                               (a.EndDate == null || a.EndDate >= DateTime.UtcNow));

                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(a => a.Location == null || a.Location == location);
                }

                var advertisements = await query
                    .Select(a => new
                    {
                        id = a.Id,
                        title = a.Title,
                        description = a.Description,
                        imagePath = a.ImagePath,
                        targetUrl = a.TargetUrl,
                        location = a.Location
                    })
                    .ToListAsync();

                return Ok(advertisements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active advertisements");
                return StatusCode(500, new { message = "Error retrieving advertisements" });
            }
        }

        /// <summary>
        /// Delete advertisement (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAdvertisement(int id)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                {
                    return NotFound(new { message = "Advertisement not found" });
                }

                // Delete associated image file
                DeleteImageFile(advertisement.ImagePath);

                // Delete from database
                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Advertisement {Id} '{Title}' deleted successfully", id, advertisement.Title);

                return Ok(new { message = "Advertisement deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting advertisement {Id}", id);
                return StatusCode(500, new { message = "Error deleting advertisement" });
            }
        }

        /// <summary>
        /// Toggle advertisement active status (admin only)
        /// </summary>
        [HttpPut("{id}/toggle")]
        [Authorize]
        public async Task<IActionResult> ToggleAdvertisement(int id)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                {
                    return NotFound(new { message = "Advertisement not found" });
                }

                advertisement.IsActive = !advertisement.IsActive;
                advertisement.LastModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Advertisement {Id} '{Title}' toggled to {Status}", 
                    id, advertisement.Title, advertisement.IsActive ? "Active" : "Inactive");

                return Ok(new { 
                    success = true, 
                    isActive = advertisement.IsActive,
                    message = $"Advertisement {(advertisement.IsActive ? "activated" : "deactivated")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling advertisement {Id}", id);
                return StatusCode(500, new { message = "Error toggling advertisement" });
            }
        }

        /// <summary>
        /// Bulk toggle advertisements (admin only)
        /// </summary>
        [HttpPut("bulk-toggle")]
        [Authorize]
        public async Task<IActionResult> BulkToggleAdvertisements([FromBody] BulkToggleRequest request)
        {
            try
            {
                var advertisements = await _context.Advertisements
                    .Where(a => request.Ids.Contains(a.Id))
                    .ToListAsync();

                if (!advertisements.Any())
                {
                    return NotFound(new { message = "No advertisements found" });
                }

                foreach (var ad in advertisements)
                {
                    ad.IsActive = !ad.IsActive;
                    ad.LastModified = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk toggle completed for {Count} advertisements", advertisements.Count);

                return Ok(new { 
                    success = true, 
                    count = advertisements.Count,
                    message = $"Successfully toggled {advertisements.Count} advertisements"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk toggle operation");
                return StatusCode(500, new { message = "Error in bulk toggle operation" });
            }
        }

        /// <summary>
        /// Bulk update advertisement status (admin only)
        /// </summary>
        [HttpPut("bulk-status")]
        [Authorize]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusRequest request)
        {
            try
            {
                var advertisements = await _context.Advertisements
                    .Where(a => request.Ids.Contains(a.Id))
                    .ToListAsync();

                if (!advertisements.Any())
                {
                    return NotFound(new { message = "No advertisements found" });
                }

                foreach (var ad in advertisements)
                {
                    ad.Status = request.Status;
                    ad.LastModified = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk status update to '{Status}' completed for {Count} advertisements", 
                    request.Status, advertisements.Count);

                return Ok(new { 
                    success = true, 
                    count = advertisements.Count,
                    message = $"Successfully updated status to '{request.Status}' for {advertisements.Count} advertisements"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk status update operation");
                return StatusCode(500, new { message = "Error in bulk status update operation" });
            }
        }

        /// <summary>
        /// Bulk delete advertisements (admin only)
        /// </summary>
        [HttpDelete("bulk-delete")]
        [Authorize]
        public async Task<IActionResult> BulkDeleteAdvertisements([FromBody] BulkDeleteRequest request)
        {
            try
            {
                var advertisements = await _context.Advertisements
                    .Where(a => request.Ids.Contains(a.Id))
                    .ToListAsync();

                if (!advertisements.Any())
                {
                    return NotFound(new { message = "No advertisements found" });
                }

                // Delete associated image files
                foreach (var ad in advertisements)
                {
                    DeleteImageFile(ad.ImagePath);
                }

                // Delete from database
                _context.Advertisements.RemoveRange(advertisements);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk delete completed for {Count} advertisements", advertisements.Count);

                return Ok(new { 
                    success = true, 
                    count = advertisements.Count,
                    message = $"Successfully deleted {advertisements.Count} advertisements"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete operation");
                return StatusCode(500, new { message = "Error in bulk delete operation" });
            }
        }

        /// <summary>
        /// Get advertisement statistics (admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = new
                {
                    totalAds = await _context.Advertisements.CountAsync(),
                    activeAds = await _context.Advertisements.CountAsync(a => a.IsActive),
                    totalClicks = await _context.Advertisements.SumAsync(a => a.ClickCount),
                    topPerforming = await _context.Advertisements
                        .Where(a => a.IsActive)
                        .OrderByDescending(a => a.ClickCount)
                        .Take(5)
                        .Select(a => new
                        {
                            id = a.Id,
                            title = a.Title,
                            clicks = a.ClickCount,
                            imagePath = a.ImagePath
                        })
                        .ToListAsync(),
                    recentAds = await _context.Advertisements
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(10)
                        .Select(a => new
                        {
                            id = a.Id,
                            title = a.Title,
                            isActive = a.IsActive,
                            clicks = a.ClickCount,
                            createdAt = a.CreatedAt
                        })
                        .ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement statistics");
                return StatusCode(500, new { message = "Error retrieving statistics" });
            }
        }

        private void DeleteImageFile(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted image file: {ImagePath}", imagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting image file: {ImagePath}", imagePath);
            }
        }
    }

    public class TrackClickRequest
    {
        public int AdvertisementId { get; set; }
    }

    public class BulkToggleRequest
    {
        public int[] Ids { get; set; } = Array.Empty<int>();
    }

    public class BulkStatusRequest
    {
        public int[] Ids { get; set; } = Array.Empty<int>();
        public string Status { get; set; } = string.Empty;
    }

    public class BulkDeleteRequest
    {
        public int[] Ids { get; set; } = Array.Empty<int>();
    }
}