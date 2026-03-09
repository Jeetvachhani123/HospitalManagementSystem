namespace HospitalMS.BL.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}