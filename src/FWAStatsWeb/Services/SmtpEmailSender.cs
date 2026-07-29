using FWAStatsWeb.Logic;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FWAStatsWeb.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> optionsAccessor, ILogger<SmtpEmailSender> logger)
    {
        _options = optionsAccessor.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        // .NET 10 tightened MailAddress parsing (consecutive dots are now rejected), and
        // recipients come from user input, so parse without throwing. Callers such as
        // ForgotPassword must not turn a bad stored address into a 500 -- that would also
        // reveal that the account exists.
        if (!MailAddress.TryCreate(email, out var recipient))
        {
            _logger.LogWarning("Not sending {Subject}: recipient address is not valid", subject);
            return;
        }

        if (!MailAddress.TryCreate(_options.SenderEmail, _options.SenderName, out var sender))
        {
            _logger.LogError("Not sending {Subject}: configured Smtp:SenderEmail {SenderEmail} is not valid",
                subject, _options.SenderEmail);
            return;
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        using var mailMessage = new MailMessage
        {
            From = sender,
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        mailMessage.To.Add(recipient);

        await client.SendMailAsync(mailMessage);
    }
}
