using System.ComponentModel.DataAnnotations;

namespace CaptivePortal.Web.Models
{
    public class PortalUser
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Location { get; set; }

        public DateTime ConnectedAt { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public bool AcceptedTerms { get; set; }

        public DateTime? TermsAcceptedAt { get; set; }
    }
}