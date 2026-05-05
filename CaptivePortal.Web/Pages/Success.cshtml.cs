using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaptivePortal.Web.Pages;

public class SuccessModel : PageModel
{
    public string Email { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public string Location { get; set; } = string.Empty;

    public void OnGet()
    {
        // These would typically come from session or query parameters
        Email = Request.Query["email"].ToString() ?? "Connected User";
        ConnectedAt = DateTime.Now;
        Location = Request.Query["location"].ToString() ?? "Stadium";
    }
}