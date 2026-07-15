using System.Net;
using System.Text.Encodings.Web;

namespace SpendiTrackWeb.Services.Email;

public static class EmailTemplates
{
    public static string Confirmation(string userName, string confirmUrl)
    {
        return Build(
            preheader: "Confirm your SpendiTrack account to start tracking expenses.",
            headline: "Confirm your email",
            bodyHtml: $"Hi <strong>{Encode(userName)}</strong>,<br/><br/>" +
                      "Thanks for joining SpendiTrack. Confirm your email to activate your account and start tracking spending.",
            buttonLabel: "Confirm email",
            buttonUrl: confirmUrl,
            footerNote: "If you did not create a SpendiTrack account, you can ignore this email.");
    }

    public static string PasswordReset(string userName, string resetUrl)
    {
        return Build(
            preheader: "Reset your SpendiTrack password.",
            headline: "Reset your password",
            bodyHtml: $"Hi <strong>{Encode(userName)}</strong>,<br/><br/>" +
                      "We received a request to reset your SpendiTrack password. Use the button below to choose a new one.",
            buttonLabel: "Reset password",
            buttonUrl: resetUrl,
            footerNote: "If you did not request a password reset, you can ignore this email.");
    }

    public static string PasswordResetCode(string userName, string resetCode)
    {
        return Build(
            preheader: "Your SpendiTrack password reset code.",
            headline: "Password reset code",
            bodyHtml: $"Hi <strong>{Encode(userName)}</strong>,<br/><br/>" +
                      "Use this code to reset your SpendiTrack password:<br/><br/>" +
                      // resetCode may already be HTML-encoded by Identity.
                      $"<div style=\"font-size:28px;font-weight:700;letter-spacing:4px;color:#2563eb;\">{resetCode}</div>",
            buttonLabel: null,
            buttonUrl: null,
            footerNote: "If you did not request a password reset, you can ignore this email.");
    }

    private static string Build(
        string preheader,
        string headline,
        string bodyHtml,
        string? buttonLabel,
        string? buttonUrl,
        string footerNote)
    {
        var buttonBlock = string.Empty;
        if (!string.IsNullOrWhiteSpace(buttonLabel) && !string.IsNullOrWhiteSpace(buttonUrl))
        {
            // buttonUrl may already be HTML-encoded by Identity; leave as-is for href.
            buttonBlock = $"""
                <tr>
                  <td align="center" style="padding:8px 0 28px;">
                    <a href="{buttonUrl}"
                       style="display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;
                              font-family:Segoe UI,Arial,sans-serif;font-size:16px;font-weight:600;
                              padding:14px 28px;border-radius:8px;">
                      {WebUtility.HtmlEncode(buttonLabel)}
                    </a>
                  </td>
                </tr>
                <tr>
                  <td style="padding:0 0 24px;font-family:Segoe UI,Arial,sans-serif;font-size:13px;line-height:1.5;color:#64748b;">
                    Or paste this link into your browser:<br/>
                    <a href="{buttonUrl}" style="color:#2563eb;word-break:break-all;">{buttonUrl}</a>
                  </td>
                </tr>
                """;
        }

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>SpendiTrack</title>
            </head>
            <body style="margin:0;padding:0;background:#0f172a;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;">{WebUtility.HtmlEncode(preheader)}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#0f172a;padding:32px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:#111827;border:1px solid #1e293b;border-radius:16px;overflow:hidden;">
                      <tr>
                        <td style="background:linear-gradient(135deg,#1d4ed8,#2563eb);padding:28px 32px;">
                          <div style="font-family:Segoe UI,Arial,sans-serif;font-size:22px;font-weight:700;color:#ffffff;letter-spacing:0.2px;">
                            SpendiTrack
                          </div>
                          <div style="font-family:Segoe UI,Arial,sans-serif;font-size:13px;color:#dbeafe;margin-top:6px;">
                            Track spending. Stay in control.
                          </div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:32px;">
                          <h1 style="margin:0 0 16px;font-family:Segoe UI,Arial,sans-serif;font-size:24px;line-height:1.3;color:#f8fafc;">
                            {WebUtility.HtmlEncode(headline)}
                          </h1>
                          <div style="font-family:Segoe UI,Arial,sans-serif;font-size:15px;line-height:1.6;color:#cbd5e1;margin-bottom:8px;">
                            {bodyHtml}
                          </div>
                          {buttonBlock}
                          <div style="border-top:1px solid #1e293b;padding-top:18px;font-family:Segoe UI,Arial,sans-serif;font-size:12px;line-height:1.5;color:#64748b;">
                            {WebUtility.HtmlEncode(footerNote)}
                          </div>
                        </td>
                      </tr>
                    </table>
                    <div style="max-width:560px;margin-top:16px;font-family:Segoe UI,Arial,sans-serif;font-size:12px;color:#64748b;">
                      © {DateTime.UtcNow.Year} SpendiTrack
                    </div>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
