using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (_smtpSettings.Server == null)
        {
            throw new Exception("SMTP Server settings are not configured.");
        }

        using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
        {
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

            var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail!, _smtpSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}