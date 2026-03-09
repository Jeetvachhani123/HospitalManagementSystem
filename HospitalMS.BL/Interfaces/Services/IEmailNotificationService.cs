namespace HospitalMS.BL.Interfaces.Services;

public interface IEmailNotificationService
{
    Task SendAppointmentRequestEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime);

    Task SendAppointmentApprovedEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime);

    Task SendAppointmentRejectedEmailAsync(string patientEmail, string patientName, string doctorName, string rejectionReason);

    Task SendAppointmentReminderEmailAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeSpan startTime);

    Task SendAppointmentCompletedEmailAsync(string patientEmail, string patientName, string doctorName, string? diagnosis, string? prescription);

    Task SendRescheduleRequestEmailAsync(string doctorEmail, string patientName, DateTime currentDate, DateTime requestedDate, string reason);

    Task SendDoctorUnavailabilityNotificationAsync(string patientEmail, string patientName, string doctorName, DateTime startDate, DateTime endDate, string reason);
}

public interface ISmsNotificationService
{
    Task SendAppointmentConfirmationSmsAsync(string phoneNumber, string doctorName, DateTime appointmentDate);

    Task SendAppointmentReminderSmsAsync(string phoneNumber, string doctorName, DateTime appointmentDate);

    Task SendAppointmentCancelledSmsAsync(string phoneNumber, string reason);
}