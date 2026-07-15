using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public bool ShowFarewell { get; set; }
        public string? DisplayName { get; set; }
        public string RedirectUrl { get; set; } = "/";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                DisplayName = user?.UserName ?? User.Identity.Name;
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            ShowFarewell = true;
            RedirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/")!;
            return Page();
        }
    }
}
