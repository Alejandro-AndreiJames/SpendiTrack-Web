namespace SpendiTrackWeb.Models;

public class AuthSuccessLandingViewModel
{
    public string Title { get; set; } = "All set";
    public string Lead { get; set; } = string.Empty;
    public string StatusText { get; set; } = "Redirecting";
    public string RedirectUrl { get; set; } = "/";
    public string ContinueLabel { get; set; } = "Continue now";
}
