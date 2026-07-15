using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace SpendiTrackWeb.Services.Email;

/// <summary>
/// Identity UI (forgot password, etc.) uses IEmailSender&lt;TUser&gt;.
/// Link/code arguments are already HTML-encoded by Identity — do not encode them again.
/// </summary>
public class IdentityEmailSender : IEmailSender<IdentityUser>
{
    private readonly IEmailSender _emailSender;

    public IdentityEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        var name = user.UserName ?? email;
        return _emailSender.SendEmailAsync(
            email,
            "Confirm your SpendiTrack account",
            EmailTemplates.Confirmation(name, confirmationLink));
    }

    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink)
    {
        var name = user.UserName ?? email;
        return _emailSender.SendEmailAsync(
            email,
            "Reset your SpendiTrack password",
            EmailTemplates.PasswordReset(name, resetLink));
    }

    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode)
    {
        var name = user.UserName ?? email;
        return _emailSender.SendEmailAsync(
            email,
            "Reset your SpendiTrack password",
            EmailTemplates.PasswordResetCode(name, resetCode));
    }
}
