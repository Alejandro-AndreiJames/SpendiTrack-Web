using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginSuccessModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public LoginSuccessModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string? DisplayName { get; set; }
        public string RedirectUrl { get; set; } = "/";

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("./Login", new { returnUrl });

            var user = await _userManager.GetUserAsync(User);
            DisplayName = user?.UserName ?? User.Identity?.Name;
            RedirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/")!;
            return Page();
        }
    }
}
