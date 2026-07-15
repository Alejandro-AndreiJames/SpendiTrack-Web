using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpendiTrackWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        public bool Succeeded { get; set; }
        public string? DisplayName { get; set; }
        public string RedirectUrl { get; set; } = "/";

        public void OnGet()
        {
            DisplayName = TempData["ResetPasswordUser"] as string;
            Succeeded = !string.IsNullOrWhiteSpace(DisplayName) || User.Identity?.IsAuthenticated == true;
            if (string.IsNullOrWhiteSpace(DisplayName) && User.Identity?.IsAuthenticated == true)
                DisplayName = User.Identity.Name;

            RedirectUrl = Url.Content("~/")!;
        }
    }
}
