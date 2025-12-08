using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    // Use IOptions<T> to get configuration settings securely
    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (_smtpSettings.Server == null)
        {
            // Handle case where settings are not configured
            throw new System.Exception("SMTP Server settings are not configured.");
        }

        using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
        {
            // Configure SMTP client for authentication and security
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