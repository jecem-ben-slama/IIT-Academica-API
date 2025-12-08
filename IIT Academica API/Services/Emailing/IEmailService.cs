using System.Threading.Tasks;

public interface IEmailService
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The HTML body of the email (e.g., containing the reset link).</param>
    Task SendEmailAsync(string toEmail, string subject, string body);
}