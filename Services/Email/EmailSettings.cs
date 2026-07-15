namespace SpendiTrackWeb.Services.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// When true (or when Host is set), send via SMTP. Otherwise write HTML files under App_Data/emails.
    /// </summary>
    public bool UseSmtp { get; set; }

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@spenditrack.local";
    public string FromDisplayName { get; set; } = "SpendiTrack";

    public bool IsSmtpConfigured =>
        UseSmtp
        && !string.IsNullOrWhiteSpace(Host)
        && !string.IsNullOrWhiteSpace(FromAddress)
        && !string.IsNullOrWhiteSpace(UserName)
        && !string.IsNullOrWhiteSpace(Password);
}
