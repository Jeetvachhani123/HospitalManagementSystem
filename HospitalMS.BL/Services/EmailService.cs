using HospitalMS.BL.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace HospitalMS.BL.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromAddress;
    private readonly string _fromName;
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _fromAddress = _configuration["Email:FromAddress"] ?? "noreply@hospitalms.com";
        _fromName = _configuration["Email:FromName"] ?? "Hospital Management System";
    }

    // send email
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPortString = _configuration["Email:SmtpPort"];
            int smtpPort = int.TryParse(smtpPortString, out int port) ? port : 587;
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            using (var client = new SmtpClient())
            {
                if (string.IsNullOrEmpty(smtpServer) || smtpServer == "localhost")
                {
                    var emailDropPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailDrop");
                    if (!Directory.Exists(emailDropPath))
                    {
                        Directory.CreateDirectory(emailDropPath);
                    }
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = emailDropPath;
                    client.Host = "localhost";
                }
                else
                {
                    client.Host = smtpServer;
                    client.Port = smtpPort;
                    client.EnableSsl = true;
                    if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                    {
                        client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    }
                }
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromAddress, _fromName);
                    message.To.Add(new MailAddress(to));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    await client.SendMailAsync(message);
                }
            }
            _logger.LogInformation($"Email sent successfully to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}");
        }
    }
}