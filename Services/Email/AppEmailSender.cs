using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace SpendiTrackWeb.Services.Email;

/// <summary>
/// Sends mail via SMTP when configured; otherwise writes HTML under App_Data/emails for local/dev.
/// </summary>
public class AppEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AppEmailSender> _logger;

    public AppEmailSender(
        IOptions<EmailSettings> options,
        IWebHostEnvironment environment,
        ILogger<AppEmailSender> logger)
    {
        _settings = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (_settings.IsSmtpConfigured)
        {
            await SendSmtpAsync(email, subject, htmlMessage);
            return;
        }

        if (_settings.UseSmtp)
        {
            _logger.LogError(
                "Email:UseSmtp is true but Host/UserName/Password/FromAddress are incomplete. " +
                "Falling back to App_Data/emails. Configure secrets with: " +
                "dotnet user-secrets set \"Email:Host\" \"smtp.gmail.com\"");
        }

        await WriteDevEmailAsync(email, subject, htmlMessage);
    }

    private async Task SendSmtpAsync(string email, string subject, string htmlMessage)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(email);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} with subject {Subject}.", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP send failed for {Email}. Subject: {Subject}", email, subject);
            throw;
        }
    }

    private async Task WriteDevEmailAsync(string email, string subject, string htmlMessage)
    {
        var folder = Path.Combine(_environment.ContentRootPath, "App_Data", "emails");
        Directory.CreateDirectory(folder);

        var safeEmail = string.Join("_", email.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{safeEmail}.html";
        var path = Path.Combine(folder, fileName);

        var document = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"><title>{WebUtility.HtmlEncode(subject)}</title></head>
            <body>
              <p><strong>To:</strong> {WebUtility.HtmlEncode(email)}</p>
              <p><strong>Subject:</strong> {WebUtility.HtmlEncode(subject)}</p>
              <hr />
              {htmlMessage}
            </body>
            </html>
            """;

        await File.WriteAllTextAsync(path, document);
        _logger.LogWarning(
            "SMTP not configured. Wrote email for {Email} to {Path}. Subject: {Subject}",
            email,
            path,
            subject);
    }
}
