using System.ComponentModel.DataAnnotations;

namespace CaptivePortal.Web.Models
{
    public class Campaign
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal? Budget { get; set; }

        [StringLength(500)]
        public string? TargetAudience { get; set; }

        public List<Advertisement> Advertisements { get; set; } = new();
    }
}