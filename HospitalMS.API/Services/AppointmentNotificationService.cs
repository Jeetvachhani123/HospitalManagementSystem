using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace HospitalMS.API.Services;

public class AppointmentNotificationService : IAppointmentNotificationService
{
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;
    private readonly ILogger<AppointmentNotificationService> _logger;
    public AppointmentNotificationService(IHubContext<Hubs.NotificationHub> hubContext, ILogger<AppointmentNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyAppointmentRequestedAsync(int doctorId, int appointmentId, string patientName, DateTime appointmentDate)
    {
        try
        {
            await _hubContext.Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", new { type = "AppointmentRequested", appointmentId, patientName, appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"), message = $"New appointment request from {patientName}", timestamp = DateTime.UtcNow });
            _logger.LogInformation($"New appointment request notification sent to doctor {doctorId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment request notification to doctor {doctorId}");
        }
    }

    public async Task NotifyAppointmentApprovedAsync(int patientId, int appointmentId, string doctorName, DateTime appointmentDate)
    {
        try
        {
            await _hubContext.Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", new { type = "AppointmentApproved", appointmentId, doctorName, appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"), message = $"Your appointment with Dr. {doctorName} has been approved!", timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Appointment approved notification sent to patient {patientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment approved notification to patient {patientId}");
        }
    }

    public async Task NotifyAppointmentRejectedAsync(int patientId, int appointmentId, string doctorName, string rejectionReason)
    {
        try
        {
            await _hubContext.Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", new { type = "AppointmentRejected", appointmentId, doctorName, rejectionReason, message = $"Your appointment with Dr. {doctorName} was rejected: {rejectionReason}", timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Appointment rejected notification sent to patient {patientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment rejected notification to patient {patientId}");
        }
    }

    public async Task NotifyAppointmentCompletedAsync(int patientId, int appointmentId, string doctorName, string? diagnosis)
    {
        try
        {
            await _hubContext.Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", new { type = "AppointmentCompleted", appointmentId, doctorName, diagnosis, message = $"Your appointment with Dr. {doctorName} has been completed.", timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Appointment completed notification sent to patient {patientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment completed notification to patient {patientId}");
        }
    }

    public async Task NotifyAppointmentCancelledAsync(int doctorId, int patientId, int appointmentId, string? cancellationReason)
    {
        try
        {
            var notification = new { type = "AppointmentCancelled", appointmentId, reason = cancellationReason, message = "An appointment has been cancelled.", timestamp = DateTime.UtcNow };
            await _hubContext.Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", notification);
            await _hubContext.Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", notification);
            _logger.LogInformation($"Appointment cancelled notification sent to doctor {doctorId} and patient {patientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment cancelled notification");
        }
    }

    public async Task NotifyAppointmentRescheduledAsync(int doctorId, int appointmentId, DateTime oldDate, DateTime newDate)
    {
        try
        {
            await _hubContext.Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", new { type = "AppointmentRescheduled", appointmentId, oldDate = oldDate.ToString("yyyy-MM-dd HH:mm"), newDate = newDate.ToString("yyyy-MM-dd HH:mm"), message = "An appointment has been rescheduled.", timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Appointment rescheduled notification sent to doctor {doctorId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending appointment rescheduled notification to doctor {doctorId}");
        }
    }

    public async Task UpdatePendingCountAsync(int doctorId, int pendingCount)
    {
        try
        {
            await _hubContext.Clients.Group($"doctor_{doctorId}").SendAsync("PendingCountUpdated", new { count = pendingCount, timestamp = DateTime.UtcNow });
            _logger.LogInformation($"Pending count update sent to doctor {doctorId}: {pendingCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending pending count update to doctor {doctorId}");
        }
    }

    public async Task BroadcastDashboardUpdateAsync(int totalAppointments, int todayAppointments, int pendingApprovals)
    {
        try
        {
            var update = new { type = "DashboardUpdate", totalAppointments, todayAppointments, pendingApprovals, timestamp = DateTime.UtcNow };
            await _hubContext.Clients.Group("admins").SendAsync("DashboardUpdated", update);
            await _hubContext.Clients.Group("doctors").SendAsync("DashboardUpdated", update);
            _logger.LogInformation("Dashboard update broadcast sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting dashboard update");
        }
    }
}