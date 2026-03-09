using Microsoft.AspNetCore.SignalR;

namespace HospitalMS.API.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    // send to all clients
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    // send to specific user
    public async Task SendToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    // join a group
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"Connection {Context.ConnectionId} joined group {groupName}");
    }

    // leave a group
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"Connection {Context.ConnectionId} left group {groupName}");
    }

    // notify appointment request
    public async Task NotifyAppointmentRequest(int doctorId, int appointmentId, string patientName, DateTime appointmentDate)
    {
        var notification = new { type = "AppointmentRequested", appointmentId, patientName, appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"), timestamp = DateTime.UtcNow };
        await Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"New appointment request notification sent to doctor {doctorId}");
    }

    // notify appointment approved
    public async Task NotifyAppointmentApproved(int patientId, int appointmentId, string doctorName, DateTime appointmentDate)
    {
        var notification = new { type = "AppointmentApproved", appointmentId, doctorName, appointmentDate = appointmentDate.ToString("yyyy-MM-dd HH:mm"), message = $"Your appointment with Dr. {doctorName} has been approved!", timestamp = DateTime.UtcNow };
        await Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment approved notification sent to patient {patientId}");
    }

    // notify appointment rejected
    public async Task NotifyAppointmentRejected(int patientId, int appointmentId, string doctorName, string rejectionReason)
    {
        var notification = new { type = "AppointmentRejected", appointmentId, doctorName, rejectionReason, message = $"Your appointment with Dr. {doctorName} was rejected: {rejectionReason}", timestamp = DateTime.UtcNow };
        await Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment rejected notification sent to patient {patientId}");
    }

    // notify appointment completed
    public async Task NotifyAppointmentCompleted(int patientId, int appointmentId, string doctorName, string? diagnosis)
    {
        var notification = new { type = "AppointmentCompleted", appointmentId, doctorName, diagnosis, message = $"Your appointment with Dr. {doctorName} has been completed.", timestamp = DateTime.UtcNow };
        await Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment completed notification sent to patient {patientId}");
    }

    // notify appointment cancelled
    public async Task NotifyAppointmentCancelled(int doctorId, int patientId, int appointmentId, string? cancellationReason)
    {
        var notification = new { type = "AppointmentCancelled", appointmentId, reason = cancellationReason, message = $"An appointment has been cancelled.", timestamp = DateTime.UtcNow };
        await Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", notification);
        await Clients.Group($"patient_{patientId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment cancelled notification sent to doctor {doctorId} and patient {patientId}");
    }

    // notify appointment rescheduled
    public async Task NotifyAppointmentRescheduled(int doctorId, int appointmentId, DateTime oldDate, DateTime newDate)
    {
        var notification = new { type = "AppointmentRescheduled", appointmentId, oldDate = oldDate.ToString("yyyy-MM-dd HH:mm"), newDate = newDate.ToString("yyyy-MM-dd HH:mm"), message = $"An appointment has been rescheduled.", timestamp = DateTime.UtcNow };
        await Clients.Group($"doctor_{doctorId}").SendAsync("AppointmentNotification", notification);
        _logger.LogInformation($"Appointment rescheduled notification sent to doctor {doctorId}");
    }

    // update pending count
    public async Task UpdatePendingCount(int doctorId, int pendingCount)
    {
        await Clients.Group($"doctor_{doctorId}").SendAsync("PendingCountUpdated", new { count = pendingCount });
    }

    // broadcast dashboard update
    public async Task BroadcastDashboardUpdate(int totalAppointments, int todayAppointments, int pendingApprovals)
    {
        var update = new { type = "DashboardUpdate", totalAppointments, todayAppointments, pendingApprovals, timestamp = DateTime.UtcNow };
        await Clients.Group("admins").SendAsync("DashboardUpdated", update);
        await Clients.Group("doctors").SendAsync("DashboardUpdated", update);
        _logger.LogInformation("Dashboard update broadcast sent");
    }

    // connection opened
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }

    // connection closed
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}