using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SpendiTrackWeb.Services.Email;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailSettings _emailSettings;

        public RegisterConfirmationModel(
            UserManager<IdentityUser> userManager,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailSettings = emailSettings.Value;
        }

        public string? Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string? EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Index");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound($"Unable to load user with email '{email}'.");

            Email = email;
            // Local/dev (no SMTP): show the same confirmation link that was emailed/written to disk.
            DisplayConfirmAccountLink = !_emailSettings.IsSmtpConfigured;
            EmailConfirmationUrl = TempData["EmailConfirmationUrl"] as string;

            return Page();
        }
    }
}
