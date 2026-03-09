using HospitalMS.BL.DTOs.Appointment;

namespace HospitalMS.BL.Interfaces.Services;

public interface IAppointmentNotificationService
{
    Task NotifyAppointmentRequestedAsync(int doctorId, int appointmentId, string patientName, DateTime appointmentDate);

    Task NotifyAppointmentApprovedAsync(int patientId, int appointmentId, string doctorName, DateTime appointmentDate);

    Task NotifyAppointmentRejectedAsync(int patientId, int appointmentId, string doctorName, string rejectionReason);

    Task NotifyAppointmentCompletedAsync(int patientId, int appointmentId, string doctorName, string? diagnosis);

    Task NotifyAppointmentCancelledAsync(int doctorId, int patientId, int appointmentId, string? cancellationReason);

    Task NotifyAppointmentRescheduledAsync(int doctorId, int appointmentId, DateTime oldDate, DateTime newDate);

    Task UpdatePendingCountAsync(int doctorId, int pendingCount);

    Task BroadcastDashboardUpdateAsync(int totalAppointments, int todayAppointments, int pendingApprovals);
}