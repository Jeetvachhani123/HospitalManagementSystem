namespace HospitalMS.BL.Interfaces.Services;

public interface IRealTimeNotificationService
{
    Task NotifyAppointmentRequest(int doctorUserId, int appointmentId, string patientName, DateTime appointmentDate);

    Task NotifyAppointmentApproved(int patientUserId, int appointmentId, string doctorName, DateTime appointmentDate);

    Task NotifyAppointmentRejected(int patientUserId, int appointmentId, string doctorName, string rejectionReason);

    Task NotifyAppointmentCompleted(int patientUserId, int appointmentId, string doctorName, string? diagnosis);

    Task NotifyAppointmentCancelled(int doctorUserId, int patientUserId, int appointmentId, string? cancellationReason);

    Task NotifyAppointmentRescheduled(int doctorUserId, int appointmentId, DateTime oldDate, DateTime newDate);

    Task UpdatePendingCount(int doctorUserId, int pendingCount);

    Task BroadcastDashboardUpdate(int totalAppointments, int todayAppointments, int pendingApprovals);
}