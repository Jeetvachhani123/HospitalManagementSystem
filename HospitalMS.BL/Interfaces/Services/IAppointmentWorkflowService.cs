using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Services;

public interface IAppointmentWorkflowService
{
    Task<AppointmentResponseDto?> RequestAppointmentAsync(AppointmentCreateDto dto, int patientId);

    Task<AppointmentResponseDto?> ApproveAppointmentAsync(int appointmentId, int doctorId);

    Task<AppointmentResponseDto?> RejectAppointmentAsync(int appointmentId, int doctorId, string rejectionReason);

    Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, int doctorId, string? diagnosis, string? prescription, string? notes);

    Task<bool> CancelAppointmentAsync(int appointmentId, int userId, string cancelledBy, string? reason = null);

    Task<AppointmentResponseDto?> RescheduleAppointmentAsync(int appointmentId, int userId, DateTime newDate, TimeSpan newStartTime, TimeSpan newEndTime);

    Task<bool> MarkAsNoShowAsync(int appointmentId);

    Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int doctorId);

    Task<IEnumerable<AppointmentStatusHistoryDto>> GetStatusHistoryAsync(int appointmentId);

    Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
}

public class TimeSlotDto
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsAvailable { get; set; }
}

public class AppointmentStatusHistoryDto
{
    public int Id { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string? PreviousApprovalStatus { get; set; }

    public string? NewApprovalStatus { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? ChangeReason { get; set; }
}