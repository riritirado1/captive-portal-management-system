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
    }
}