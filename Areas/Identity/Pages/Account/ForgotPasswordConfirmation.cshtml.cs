using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SpendiTrackWeb.Services.Email;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel : PageModel
    {
        private readonly EmailSettings _emailSettings;

        public ForgotPasswordConfirmationModel(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public bool DisplayResetLink { get; set; }
        public string? PasswordResetUrl { get; set; }

        public void OnGet()
        {
            DisplayResetLink = !_emailSettings.IsSmtpConfigured;
            PasswordResetUrl = TempData["PasswordResetUrl"] as string;
        }
    }
}
