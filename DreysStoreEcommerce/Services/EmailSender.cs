using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DreysStoreEcommerce.Services
{
    /// <summary>
    /// Handles sending e-mails. Default implementation attempts to use Gmail SMTP when configured.
    /// The original SMTP implementation (reading Smtp section) is preserved below as a commented block
    /// for reference. If Gmail settings are not present this service will log a warning and skip sending.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        /*
        // Original SMTP implementation (kept for reference, commented out because placeholder values may cause SocketException)
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");
            var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = true
            };
            return client.SendMailAsync(new MailMessage(smtp["Username"], email, subject, htmlMessage) { IsBodyHtml = true });
        }
        */

        /// <summary>
        /// Sends an email to the specified recipient using Gmail or fallback SMTP settings.
        /// If configuration contains obvious placeholders this method will log and skip sending.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="htmlMessage">HTML body of the email.</param>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Try Gmail settings first (recommended). In appsettings use: "Gmail": { "Username": "you@gmail.com", "Password": "app-password" }
            var gmail = _config.GetSection("Gmail");
            if (gmail.Exists() && !string.IsNullOrEmpty(gmail["Username"]) && !string.IsNullOrEmpty(gmail["Password"]))
            {
                // Ignore common placeholder values to avoid attempting real SMTP connections with invalid data
                var gUser = gmail["Username"]?.Trim();
                var gPass = gmail["Password"]?.Trim();
                if (string.Equals(gUser, "your@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(gPass, "your-app-password", StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogWarning("Gmail configuration contains placeholder values; skipping Gmail send.");
                }
                else
                {
                try
                {
                    using var client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential(gmail["Username"], gmail["Password"]),
                        EnableSsl = true
                    };

                    var mail = new MailMessage(gmail["Username"], email, subject, htmlMessage) { IsBodyHtml = true };
                    await client.SendMailAsync(mail);
                    _logger?.LogInformation("Sent email to {Email} via Gmail SMTP.", email);
                    return;
                }
                catch (SmtpException sx)
                {
                    _logger?.LogError(sx, "Gmail SMTP send failed for {Email}.", email);
                    // Don't rethrow; log and continue so mail failures don't break user flows
                }
                }
            }

            // If Gmail not configured, fall back to Smtp config if present
            var smtp = _config.GetSection("Smtp");
            if (smtp.Exists() && !string.IsNullOrEmpty(smtp["Host"]))
            {
                var host = smtp["Host"]?.Trim() ?? string.Empty;
                if (host.Contains("yourprovider") || host.Contains("example") || host.Contains("your"))
                {
                    _logger?.LogWarning("Configured SMTP host looks like a placeholder ('{Host}'); skipping SMTP send.", host);
                }
                else
                {
                try
                {
                    using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"] ?? "25"))
                    {
                        Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                        EnableSsl = bool.TryParse(smtp["EnableSsl"], out var ssl) ? ssl : true
                    };
                    var from = smtp["Username"] ?? gmail["Username"];
                    var mail = new MailMessage(from, email, subject, htmlMessage) { IsBodyHtml = true };
                    await client.SendMailAsync(mail);
                    _logger?.LogInformation("Sent email to {Email} via configured SMTP ({Host}).", email, smtp["Host"]);
                    return;
                }
                catch (SmtpException sx)
                {
                    _logger?.LogError(sx, "Configured SMTP send failed for {Email}.", email);
                    // Don't rethrow; log and continue
                }
                }
            }

            // No SMTP configured; log and skip sending to avoid SocketException in environments without SMTP
            _logger?.LogWarning("No SMTP or Gmail configuration found. Skipping sending email to {Email}.", email);
            return;
        }
    }

}
