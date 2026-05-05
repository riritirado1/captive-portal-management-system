using System.ComponentModel.DataAnnotations;

namespace CaptivePortal.Web.Models
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public string? TargetUrl { get; set; }

        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ClickCount { get; set; } = 0;

        // PBI#7 Enhancements
        public int? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? LastModified { get; set; }

        [StringLength(100)]  
        public string? ModifiedBy { get; set; }

        public int Priority { get; set; } = 0; // Higher numbers = higher priority

        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        public string? ImageSize { get; set; } // Store image dimensions

        public long? ImageFileSize { get; set; } // Store file size in bytes

        [StringLength(100)]
        public string? Status { get; set; } = "Draft"; // Draft, Pending Approval, Approved, Rejected

        public int ViewCount { get; set; } = 0; // Track how many times it's displayed

        public DateTime? LastDisplayed { get; set; }

        // Calculated properties
        public double ClickThroughRate => ViewCount > 0 ? (double)ClickCount / ViewCount * 100 : 0;

        public bool IsScheduled => StartDate.HasValue && StartDate > DateTime.UtcNow;

        public bool IsExpired => EndDate.HasValue && EndDate < DateTime.UtcNow;

        public bool IsCurrentlyActive => IsActive && !IsExpired && (!StartDate.HasValue || StartDate <= DateTime.UtcNow);
    }
}