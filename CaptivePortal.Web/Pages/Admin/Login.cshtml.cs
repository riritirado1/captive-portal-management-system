using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CaptivePortal.Web.Pages.Admin
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            // Clear any existing external login errors
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear existing cookies to ensure clean login
            await _signInManager.SignOutAsync();

            ReturnUrl = returnUrl ?? "/Admin";
        }

        public async Task<IActionResult> OnPostLoginAsync(string? returnUrl = null)
        {
            returnUrl ??= "/Admin";

            if (ModelState.IsValid)
            {
                // Check if user exists and is an admin
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ErrorMessage = "Invalid login credentials. Access denied.";
                    _logger.LogWarning($"Failed login attempt for non-existent user: {Input.Email}");
                    return Page();
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, 
                    Input.Password, 
                    Input.RememberMe, 
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Admin user {Input.Email} logged in successfully.");
                    return LocalRedirect(returnUrl);
                }
                
                if (result.RequiresTwoFactor)
                {
                    ErrorMessage = "Two-factor authentication is required.";
                    return Page();
                }
                
                if (result.IsLockedOut)
                {
                    ErrorMessage = "Account is locked due to multiple failed login attempts. Please try again later.";
                    _logger.LogWarning($"Account locked for user: {Input.Email}");
                    return Page();
                }
                
                if (result.IsNotAllowed)
                {
                    ErrorMessage = "Account is not allowed to sign in.";
                    return Page();
                }

                // Default error for failed login
                ErrorMessage = "Invalid login credentials. Please check your email and password.";
                _logger.LogWarning($"Failed login attempt for user: {Input.Email}");
                return Page();
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin user logged out.");
            return RedirectToPage("/Index");
        }
    }
}