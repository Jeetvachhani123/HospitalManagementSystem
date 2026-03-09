using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace HospitalMS.API.Hubs
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public RealTimeNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyAppointmentRequest(int doctorId, int appointmentId, string patientName, DateTime appointmentDate)
        {
            return _hubContext.Clients.User(doctorId.ToString()).SendAsync("AppointmentRequested", new { appointmentId, patientName, appointmentDate });
        }

        public Task NotifyAppointmentApproved(int patientId, int appointmentId, string doctorName, DateTime appointmentDate)
        {
            return _hubContext.Clients.User(patientId.ToString()).SendAsync("AppointmentApproved", new { appointmentId, doctorName, appointmentDate });
        }

        public Task NotifyAppointmentRejected(int patientId, int appointmentId, string doctorName, string rejectionReason)
        {
            return _hubContext.Clients.User(patientId.ToString()).SendAsync("AppointmentRejected", new { appointmentId, doctorName, rejectionReason });
        }

        public Task NotifyAppointmentCompleted(int patientId, int appointmentId, string doctorName, string? diagnosis)
        {
            return _hubContext.Clients.User(patientId.ToString()).SendAsync("AppointmentCompleted", new { appointmentId, doctorName, diagnosis });
        }

        public Task NotifyAppointmentCancelled(int doctorId, int patientId, int appointmentId, string? cancellationReason)
        {
            return _hubContext.Clients.Users(doctorId.ToString(), patientId.ToString()).SendAsync("AppointmentCancelled", new { appointmentId, cancellationReason });
        }

        public Task NotifyAppointmentRescheduled(int doctorId, int appointmentId, DateTime oldDate, DateTime newDate)
        {
            return _hubContext.Clients.User(doctorId.ToString()).SendAsync("AppointmentRescheduled", new { appointmentId, oldDate, newDate });
        }

        public Task UpdatePendingCount(int doctorId, int pendingCount)
        {
            return _hubContext.Clients.User(doctorId.ToString()).SendAsync("UpdatePendingCount", pendingCount);
        }

        public Task BroadcastDashboardUpdate(int totalAppointments, int todayAppointments, int pendingApprovals)
        {
            return _hubContext.Clients.All.SendAsync("DashboardUpdate", new { totalAppointments, todayAppointments, pendingApprovals });
        }
    }
}