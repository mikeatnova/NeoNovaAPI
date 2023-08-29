using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        string email = _configuration["Email:ServiceUsername"];
        string password = _configuration["Email:Password"];

        SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true,
        };

        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(email),
            Subject = subject,
            Body = body,
        };

        mailMessage.To.Add(toEmail);
        await client.SendMailAsync(mailMessage);
    }
}