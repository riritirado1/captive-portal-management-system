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

        // Wi-Fi Access Grant Fields (PBI#5)
        public bool AccessGranted { get; set; } = false;

        public DateTime? AccessGrantedAt { get; set; }

        public DateTime? AccessExpiresAt { get; set; }

        public string? SessionId { get; set; }

        public string? MacAddress { get; set; }

        public bool IsSessionActive => AccessExpiresAt.HasValue && AccessExpiresAt > DateTime.UtcNow;

        public string? RedirectUrl { get; set; }

        public int ReconnectionCount { get; set; } = 1;

        public DateTime? LastActivityAt { get; set; }
    }
}