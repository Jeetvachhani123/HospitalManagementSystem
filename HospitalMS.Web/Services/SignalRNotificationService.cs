using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HospitalMS.Web.Services;

public class SignalRNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;
    // inject hub and logger
    public SignalRNotificationService(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    // notify appointment requested
    public async Task NotifyAppointmentRequest(int doctorUserId, int appointmentId, string patientName, DateTime appointmentDate)
    {
        var notification = new
        {
            type = "AppointmentRequested",
            appointmentId,
            patientName,
            appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"),
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"doctor_{doctorUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"New appointment request notification sent to doctor user {doctorUserId}");
    }

    // notify appointment approved
    public async Task NotifyAppointmentApproved(int patientUserId, int appointmentId, string doctorName, DateTime appointmentDate)
    {
        var notification = new
        {
            type = "AppointmentApproved",
            appointmentId,
            doctorName,
            appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"),
            message = $"Your appointment with Dr. {doctorName} has been approved!",
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"patient_{patientUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment approved notification sent to patient user {patientUserId}");
    }

    // notify appointment rejected
    public async Task NotifyAppointmentRejected(int patientUserId, int appointmentId, string doctorName, string rejectionReason)
    {
        var notification = new
        {
            type = "AppointmentRejected",
            appointmentId,
            doctorName,
            rejectionReason,
            message = $"Your appointment with Dr. {doctorName} was rejected: {rejectionReason}",
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"patient_{patientUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment rejected notification sent to patient user {patientUserId}");
    }

    // notify appointment completed
    public async Task NotifyAppointmentCompleted(int patientUserId, int appointmentId, string doctorName, string? diagnosis)
    {
        var notification = new
        {
            type = "AppointmentCompleted",
            appointmentId,
            doctorName,
            diagnosis,
            message = $"Your appointment with Dr. {doctorName} has been completed.",
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"patient_{patientUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment completed notification sent to patient user {patientUserId}");
    }

    // notify appointment cancelled
    public async Task NotifyAppointmentCancelled(int doctorUserId, int patientUserId, int appointmentId, string? cancellationReason)
    {
        var notification = new
        {
            type = "AppointmentCancelled",
            appointmentId,
            reason = cancellationReason,
            message = $"An appointment has been cancelled.",
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"doctor_{doctorUserId}").SendAsync("AppointmentNotification", notification);
        await _hubContext.Clients.Group($"patient_{patientUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment cancelled notification sent to doctor user {doctorUserId} and patient user {patientUserId}");
    }

    // notify appointment rescheduled
    public async Task NotifyAppointmentRescheduled(int doctorUserId, int appointmentId, DateTime oldDate, DateTime newDate)
    {
        var notification = new
        {
            type = "AppointmentRescheduled",
            appointmentId,
            oldDate = oldDate.ToString("yyyy-MM-dd HH:mm"),
            newDate = newDate.ToString("yyyy-MM-dd HH:mm"),
            message = $"An appointment has been rescheduled.",
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group($"doctor_{doctorUserId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment rescheduled notification sent to doctor user {doctorUserId}");
    }

    // update pending count
    public async Task UpdatePendingCount(int doctorUserId, int pendingCount)
    {
        await _hubContext.Clients.Group($"doctor_{doctorUserId}").SendAsync("PendingCountUpdated", new { count = pendingCount });
    }

    // broadcast dashboard update
    public async Task BroadcastDashboardUpdate(int totalAppointments, int todayAppointments, int pendingApprovals)
    {
        var update = new
        {
            type = "DashboardUpdate",
            totalAppointments,
            todayAppointments,
            pendingApprovals,
            timestamp = DateTime.UtcNow
        };
        await _hubContext.Clients.Group("admins").SendAsync("DashboardUpdated", update);
        await _hubContext.Clients.Group("doctors").SendAsync("DashboardUpdated", update);
        _logger.LogInformation("Dashboard update broadcast sent");
    }
}