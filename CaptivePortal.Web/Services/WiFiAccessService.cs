using CaptivePortal.Web.Models;
using Microsoft.Extensions.Options;

namespace CaptivePortal.Web.Services
{
    public interface IWiFiAccessService
    {
        Task<WiFiAccessResult> GrantAccessAsync(PortalUser user, string? originalUrl = null);
        Task<bool> ValidateSessionAsync(string sessionId);
        Task<bool> ExtendSessionAsync(string sessionId);
        Task<bool> RevokeAccessAsync(string sessionId);
        string DetermineRedirectUrl(PortalUser user, string? originalUrl = null);
        Task<WiFiSessionInfo?> GetSessionInfoAsync(string sessionId);
    }

    public class WiFiAccessService : IWiFiAccessService
    {
        private readonly WiFiAccessOptions _options;
        private readonly ILogger<WiFiAccessService> _logger;

        public WiFiAccessService(IOptions<WiFiAccessOptions> options, ILogger<WiFiAccessService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<WiFiAccessResult> GrantAccessAsync(PortalUser user, string? originalUrl = null)
        {
            try
            {
                // Generate unique session ID
                var sessionId = GenerateSessionId();
                var accessGrantedAt = DateTime.UtcNow;
                var accessExpiresAt = accessGrantedAt.AddHours(_options.SessionDurationHours);

                // Update user with access details
                user.AccessGranted = true;
                user.AccessGrantedAt = accessGrantedAt;
                user.AccessExpiresAt = accessExpiresAt;
                user.SessionId = sessionId;
                user.LastActivityAt = accessGrantedAt;
                user.RedirectUrl = DetermineRedirectUrl(user, originalUrl);

                // Simulate network access grant (in real implementation, this would interface with network equipment)
                await SimulateNetworkAccessGrantAsync(user);

                _logger.LogInformation(
                    "Wi-Fi access granted to {Email} from {IpAddress}. Session: {SessionId}, Expires: {ExpiresAt}",
                    user.Email, user.IpAddress, sessionId, accessExpiresAt);

                return new WiFiAccessResult
                {
                    Success = true,
                    SessionId = sessionId,
                    AccessGrantedAt = accessGrantedAt,
                    AccessExpiresAt = accessExpiresAt,
                    RedirectUrl = user.RedirectUrl,
                    NetworkName = _options.NetworkName,
                    Message = "Wi-Fi access granted successfully!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to grant Wi-Fi access to {Email}", user.Email);
                return new WiFiAccessResult
                {
                    Success = false,
                    Message = "Failed to grant Wi-Fi access. Please try again."
                };
            }
        }

        public async Task<bool> ValidateSessionAsync(string sessionId)
        {
            try
            {
                // In a real implementation, this would check with network equipment
                // For now, we'll simulate validation
                await Task.Delay(100);
                
                _logger.LogDebug("Session {SessionId} validated", sessionId);
                return !string.IsNullOrEmpty(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<bool> ExtendSessionAsync(string sessionId)
        {
            try
            {
                // Simulate session extension
                await Task.Delay(100);
                
                _logger.LogInformation("Session {SessionId} extended by {Minutes} minutes", 
                    sessionId, _options.SessionExtendMinutes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extend session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<bool> RevokeAccessAsync(string sessionId)
        {
            try
            {
                // Simulate access revocation
                await Task.Delay(100);
                
                _logger.LogInformation("Wi-Fi access revoked for session {SessionId}", sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke access for session {SessionId}", sessionId);
                return false;
            }
        }

        public string DetermineRedirectUrl(PortalUser user, string? originalUrl = null)
        {
            // If there's an original URL from the captive portal detection, use it
            if (!string.IsNullOrEmpty(originalUrl) && Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
                return originalUrl;
            }

            // Determine redirect based on email domain or user type
            var emailDomain = user.Email.Split('@').LastOrDefault()?.ToLowerInvariant();
            
            // Safe dictionary access with fallbacks
            if (emailDomain == "wtamu.edu")
            {
                // Try to determine if student or faculty based on email
                var isStudent = user.Email.Contains("student", StringComparison.OrdinalIgnoreCase);
                
                if (isStudent && _options.RedirectUrls.TryGetValue("Student", out var studentUrl))
                {
                    return studentUrl;
                }
                else if (!isStudent && _options.RedirectUrls.TryGetValue("Faculty", out var facultyUrl))
                {
                    return facultyUrl;
                }
                
                // Fallback for WTAMU emails
                if (_options.RedirectUrls.TryGetValue("Default", out var defaultUrl))
                {
                    return defaultUrl;
                }
            }
            else
            {
                // Guest users
                if (_options.RedirectUrls.TryGetValue("Guest", out var guestUrl))
                {
                    return guestUrl;
                }
            }
            
            // Final fallback
            return _options.DefaultRedirectUrl;
        }

        public async Task<WiFiSessionInfo?> GetSessionInfoAsync(string sessionId)
        {
            try
            {
                // In real implementation, this would query network equipment or database
                await Task.Delay(50);
                
                return new WiFiSessionInfo
                {
                    SessionId = sessionId,
                    IsActive = true,
                    LastActivity = DateTime.UtcNow,
                    DataUsage = new Random().Next(100, 500) // Simulated MB usage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get session info for {SessionId}", sessionId);
                return null;
            }
        }

        private async Task SimulateNetworkAccessGrantAsync(PortalUser user)
        {
            // Simulate network equipment communication delay
            await Task.Delay(200);
            
            // In a real implementation, this would:
            // 1. Add user's MAC address to allowed devices list
            // 2. Configure firewall rules
            // 3. Set bandwidth limits if applicable
            // 4. Register with RADIUS server
            // 5. Update network access control lists
            
            _logger.LogDebug("Network access configured for {Email} at {IpAddress}", 
                user.Email, user.IpAddress);
        }

        private string GenerateSessionId()
        {
            return $"WTAMU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        }
    }

    public class WiFiAccessResult
    {
        public bool Success { get; set; }
        public string? SessionId { get; set; }
        public DateTime? AccessGrantedAt { get; set; }
        public DateTime? AccessExpiresAt { get; set; }
        public string? RedirectUrl { get; set; }
        public string? NetworkName { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class WiFiSessionInfo
    {
        public string SessionId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastActivity { get; set; }
        public long DataUsage { get; set; } // MB
    }

    public class WiFiAccessOptions
    {
        public int SessionDurationHours { get; set; } = 24;
        public int MaxReconnectionsPerDay { get; set; } = 5;
        public int RedirectDelay { get; set; } = 3;
        public string DefaultRedirectUrl { get; set; } = "https://wtamu.edu";
        public string NetworkName { get; set; } = "WTAMU-WiFi";
        public bool EnableAutoRedirect { get; set; } = true;
        public int SessionExtendMinutes { get; set; } = 30;
        public Dictionary<string, string> RedirectUrls { get; set; } = new();
    }
}