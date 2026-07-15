using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SpendiTrackWeb.Services.Email;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender<IdentityUser> _emailSender;
        private readonly EmailSettings _emailSettings;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender<IdentityUser> emailSender,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed.
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme)!;

            await _emailSender.SendPasswordResetLinkAsync(
                user,
                Input.Email,
                HtmlEncoder.Default.Encode(callbackUrl));

            if (!_emailSettings.IsSmtpConfigured)
            {
                TempData["PasswordResetUrl"] = callbackUrl;
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
