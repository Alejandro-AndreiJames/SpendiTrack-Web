using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ConfirmEmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string? StatusMessage { get; set; }
        public bool Succeeded { get; set; }
        public string? DisplayName { get; set; }
        public string RedirectUrl { get; set; } = "~/";

        public async Task<IActionResult> OnGetAsync(string? userId, string? code, string? returnUrl = null)
        {
            if (userId == null || code == null)
                return RedirectToPage("/Index");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            var alreadyConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (result.Succeeded || alreadyConfirmed)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                Succeeded = true;
                DisplayName = user.UserName;
                var trackerUrl = Url.Action("Index", "Expenses") ?? "/Expenses";
                var homeUrl = Url.Content("~/") ?? "/";
                if (Url.IsLocalUrl(returnUrl)
                    && !string.Equals(returnUrl, "/", StringComparison.Ordinal)
                    && !string.Equals(returnUrl, "~/", StringComparison.Ordinal)
                    && !string.Equals(returnUrl, homeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    RedirectUrl = returnUrl!;
                }
                else
                {
                    RedirectUrl = trackerUrl;
                }

                return Page();
            }

            Succeeded = false;
            StatusMessage = "Email confirmation failed. The link may be invalid or already used.";
            return Page();
        }
    }
}
