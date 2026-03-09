using HospitalMS.BL.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace HospitalMS.BL.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationService> _logger;
    public EmailNotificationService(IEmailService emailService, ILogger<EmailNotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    // send appointment request email
    public async Task SendAppointmentRequestEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime)
    {
        try
        {
            var subject = "Appointment Request Submitted";
            var body = BuildHtmlBody($@"
                <h2>Appointment Request Received</h2>
                <p>Dear {patientName},</p>
                <p>Your appointment request has been submitted successfully.</p>
                <div style='background: #f5f5f5; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <p><strong>Doctor:</strong> Dr. {doctorName}</p>
                    <p><strong>Requested Date:</strong> {appointmentDate:MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {startTime:hh\\:mm}</p>
                </div>
                <p>The doctor will review your request and notify you of their decision shortly.</p>
                <p>Thank you for choosing our hospital.</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment request email to {patientEmail}");
        }
    }

    // send appointment approved email
    public async Task SendAppointmentApprovedEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime)
    {
        try
        {
            var subject = "Your Appointment Has Been Approved ?";
            var body = BuildHtmlBody($@"
                <h2>Appointment Confirmed</h2>
                <p>Dear {patientName},</p>
                <p>Great news! Your appointment has been approved and confirmed.</p>
                <div style='background: #e8f5e9; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4caf50;'>
                    <p><strong>Doctor:</strong> Dr. {doctorName}</p>
                    <p><strong>Date:</strong> {appointmentDate:MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {startTime:hh\\:mm}</p>
                </div>
                <p>Please arrive 10 minutes early for your appointment.</p>
                <p>If you need to reschedule, please contact us as soon as possible.</p>
                <p>Best regards,<br/>Hospital Management System</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment approved email to {patientEmail}");
        }
    }

    // send appointment rejected email
    public async Task SendAppointmentRejectedEmailAsync(string patientEmail, string patientName, string doctorName, string rejectionReason)
    {
        try
        {
            var subject = "Appointment Request Not Approved";
            var body = BuildHtmlBody($@"
                <h2>Appointment Request Update</h2>
                <p>Dear {patientName},</p>
                <p>Unfortunately, your appointment request with Dr. {doctorName} could not be approved at this time.</p>
                <div style='background: #ffebee; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #f44336;'>
                    <p><strong>Reason:</strong> {rejectionReason}</p>
                </div>
                <p>Please feel free to:</p>
                <ul>
                    <li>Request another appointment at a different time</li>
                    <li>Contact the doctor's office for more information</li>
                    <li>Request an appointment with another doctor</li>
                </ul>
                <p>We regret any inconvenience. Thank you for your understanding.</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment rejected email to {patientEmail}");
        }
    }

    // send appointment reminder email
    public async Task SendAppointmentReminderEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime)
    {
        try
        {
            var subject = "Appointment Reminder - Tomorrow at " + startTime.ToString(@"hh\:mm");
            var body = BuildHtmlBody($@"
                <h2>Appointment Reminder</h2>
                <p>Dear {patientName},</p>
                <p>This is a reminder about your upcoming appointment.</p>
                <div style='background: #fff3e0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                    <p><strong>Doctor:</strong> Dr. {doctorName}</p>
                    <p><strong>Date:</strong> {appointmentDate:MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {startTime:hh\\:mm}</p>
                </div>
                <p>Please remember to:</p>
                <ul>
                    <li>Arrive 10 minutes early</li>
                    <li>Bring any relevant medical documents</li>
                    <li>Bring your insurance card if applicable</li>
                </ul>
                <p>If you need to reschedule, please contact us immediately.</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment reminder email to {patientEmail}");
        }
    }

    // send completed appointment email
    public async Task SendAppointmentCompletedEmailAsync(string patientEmail, string patientName, string doctorName, string? diagnosis, string? prescription)
    {
        try
        {
            var diagnosisHtml = string.IsNullOrEmpty(diagnosis) ? "<em>None recorded</em>" : diagnosis;
            var prescriptionHtml = string.IsNullOrEmpty(prescription) ? "<em>None prescribed</em>" : prescription;
            var subject = "Your Appointment Summary";
            var body = BuildHtmlBody($@"
                <h2>Appointment Completed</h2>
                <p>Dear {patientName},</p>
                <p>Thank you for visiting Dr. {doctorName}. Here's a summary of your visit:</p>
                <div style='background: #e3f2fd; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <p><strong>Diagnosis:</strong></p>
                    <p>{diagnosisHtml}</p>
                    <p><strong>Prescription:</strong></p>
                    <p>{prescriptionHtml}</p>
                </div>
                <p>Please follow the doctor's recommendations and take your medications as prescribed.</p>
                <p>If you have any questions, don't hesitate to contact us.</p>
                <p>Thank you for your trust in our hospital.</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment completed email to {patientEmail}");
        }
    }

    // send reschedule request email
    public async Task SendRescheduleRequestEmailAsync(string doctorEmail, string patientName, DateTime currentDate, DateTime requestedDate, string reason)
    {
        try
        {
            var subject = "Appointment Reschedule Request";
            var body = BuildHtmlBody($@"
                <h2>Reschedule Request</h2>
                <p>A patient has requested to reschedule their appointment.</p>
                <div style='background: #f5f5f5; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <p><strong>Patient:</strong> {patientName}</p>
                    <p><strong>Original Date:</strong> {currentDate:MMMM dd, yyyy}</p>
                    <p><strong>Requested Date:</strong> {requestedDate:MMMM dd, yyyy}</p>
                    <p><strong>Reason:</strong> {reason}</p>
                </div>
                <p>Please review this request and respond accordingly.</p>
            ");
            await SendEmailAsync(doctorEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending reschedule request email to {doctorEmail}");
        }
    }

    // send unavailability notification
    public async Task SendDoctorUnavailabilityNotificationAsync(string patientEmail, string patientName, string doctorName, DateTime startDate, DateTime endDate, string reason)
    {
        try
        {
            var subject = "Doctor Unavailability Notice";
            var body = BuildHtmlBody($@"
                <h2>Doctor Unavailable</h2>
                <p>Dear {patientName},</p>
                <p>We wanted to notify you that Dr. {doctorName} will be unavailable during the following period:</p>
                <div style='background: #fff3e0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                    <p><strong>From:</strong> {startDate:MMMM dd, yyyy}</p>
                    <p><strong>To:</strong> {endDate:MMMM dd, yyyy}</p>
                    <p><strong>Reason:</strong> {reason}</p>
                </div>
                <p>If you have an appointment scheduled during this time, we will contact you to reschedule.</p>
                <p>Thank you for your understanding.</p>
            ");
            await SendEmailAsync(patientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending unavailability email to {patientEmail}");
        }
    }

    // send email via service
    private async Task SendEmailAsync(string toAddress, string subject, string htmlBody)
    {
        await _emailService.SendEmailAsync(toAddress, subject, htmlBody);
    }

    // build html email body
    private string BuildHtmlBody(string content)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    a {{ color: #2196F3; text-decoration: none; }}
                    a:hover {{ text-decoration: underline; }}
                </style>
            </head>
            <body>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    {content}
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;' />
                    <p style='font-size: 12px; color: #999;'>
                        This is an automated message from Hospital Management System.<br/>
                        Please do not reply to this email.
                    </p>
                </div>
            </body>
            </html>
        ";
    }
}