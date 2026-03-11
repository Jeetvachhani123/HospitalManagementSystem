using AutoMapper;
using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class AppointmentWorkflowService : IAppointmentWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IRealTimeNotificationService _realTimeNotificationService;
    private readonly IBillingService _billingService;
    private readonly IReportingService _reportingService;
    private readonly ILogger<AppointmentWorkflowService> _logger;
    public AppointmentWorkflowService(IUnitOfWork unitOfWork, IMapper mapper, IEmailNotificationService emailNotificationService, IRealTimeNotificationService realTimeNotificationService, IBillingService billingService, IReportingService reportingService, ILogger<AppointmentWorkflowService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailNotificationService = emailNotificationService;
        _realTimeNotificationService = realTimeNotificationService;
        _billingService = billingService;
        _reportingService = reportingService;
        _logger = logger;
    }

    // request appointment
    public async Task<AppointmentResponseDto?> RequestAppointmentAsync(AppointmentCreateDto dto, int patientId)
    {
        try
        {
            if (dto.AppointmentDate.Date < DateTime.UtcNow.Date)
                return null;
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
                return null;
            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
                return null;
            bool hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
                dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime);
            if (hasConflict)
                return null;
            if (!await ValidateDoctorWorkingHoursAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime))
                return null;
            var appointment = new Appointment { PatientId = patientId, DoctorId = dto.DoctorId, AppointmentDate = dto.AppointmentDate, StartTime = dto.StartTime, EndTime = dto.EndTime, Reason = dto.Reason, Status = AppointmentStatus.Scheduled, ApprovalStatus = AppointmentApprovalStatus.Pending, CreatedAt = DateTime.UtcNow };
            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                NewStatus = appointment.Status,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = $"PatientId:{patientId}",
                ChangeReason = "Appointment Requested"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                await _realTimeNotificationService.NotifyAppointmentRequest(doctor.UserId, appointment.Id, $"{patient.User.FirstName} {patient.User.LastName}", appointment.GetFullStartDateTime());
                await _emailNotificationService.SendAppointmentRequestEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{doctor.User.FirstName} {doctor.User.LastName}", appointment.AppointmentDate, appointment.StartTime);
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notifications");
            }
            _logger.LogInformation($"Appointment {appointment.Id} requested by patient {patientId}");
            await NotifyDashboardUpdateAsync();
            var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == dto.DoctorId && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
            await _realTimeNotificationService.UpdatePendingCount(doctor.UserId, pendingCount);
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting appointment");
            return null;
        }
    }

    // approve appointment
    public async Task<AppointmentResponseDto?> ApproveAppointmentAsync(int appointmentId, int doctorId)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;
            if (appointment.DoctorId != doctorId)
                return null;
            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Pending)
                return null;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            appointment.ApprovalStatus = AppointmentApprovalStatus.Approved;
            appointment.ApprovedByDoctorId = doctorId;
            appointment.ApprovedAt = DateTime.UtcNow;
            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = $"DoctorId:{doctorId}",
                ChangeReason = "Appointment Approved"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                if (patient != null && doctor != null)
                {
                    await _realTimeNotificationService.NotifyAppointmentApproved(patient.User.Id, appointment.Id, $"{doctor.User.FirstName} {doctor.User.LastName}", appointment.GetFullStartDateTime());
                    await _emailNotificationService.SendAppointmentApprovedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{doctor.User.FirstName} {doctor.User.LastName}", appointment.AppointmentDate, appointment.StartTime);
                }
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notifications");
            }
            _logger.LogInformation($"Appointment {appointmentId} approved by doctor {doctorId}");
            await NotifyDashboardUpdateAsync();
            var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == doctorId && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
            await _realTimeNotificationService.UpdatePendingCount(appointment.Doctor.UserId, pendingCount);
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving appointment {appointmentId}");
            return null;
        }
    }

    // reject appointment
    public async Task<AppointmentResponseDto?> RejectAppointmentAsync(int appointmentId, int doctorId, string rejectionReason)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;
            if (appointment.DoctorId != doctorId)
                return null;
            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Pending)
                return null;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            appointment.ApprovalStatus = AppointmentApprovalStatus.Rejected;
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.RejectionReason = rejectionReason;
            appointment.ApprovedByDoctorId = doctorId;
            appointment.ApprovedAt = DateTime.UtcNow;
            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = $"DoctorId:{doctorId}",
                ChangeReason = "Rejected: " + rejectionReason
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                if (patient != null && doctor != null)
                {
                    await _realTimeNotificationService.NotifyAppointmentRejected(patient.User.Id, appointment.Id, $"{doctor.User.FirstName} {doctor.User.LastName}", rejectionReason);
                    await _emailNotificationService.SendAppointmentRejectedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{doctor.User.FirstName} {doctor.User.LastName}", rejectionReason);
                }
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notifications");
            }
            _logger.LogInformation($"Appointment {appointmentId} rejected by doctor {doctorId}");
            await NotifyDashboardUpdateAsync();
            var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == doctorId && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
            await _realTimeNotificationService.UpdatePendingCount(appointment.Doctor.UserId, pendingCount);
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting appointment {appointmentId}");
            return null;
        }
    }

    // complete appointment
    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, int doctorId, string? diagnosis, string? prescription, string? notes)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;
            if (appointment.DoctorId != doctorId)
                return null;
            if (appointment.Status == AppointmentStatus.Completed)
                return null;
            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Approved)
                return null;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            appointment.Status = AppointmentStatus.Completed;
            appointment.Diagnosis = diagnosis;
            appointment.Prescription = prescription;
            appointment.Notes = notes;
            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = $"DoctorId:{doctorId}",
                ChangeReason = "Appointment Completed"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                var fee = appointment.Doctor.ConsultationFee;
                if (fee > 0)
                {
                    await _billingService.GenerateInvoiceAsync(appointmentId, fee, DateTime.UtcNow.AddDays(14));
                    _logger.LogInformation("Invoice generated for appointment {ApptId}", appointmentId);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invoice already exists for appointment {ApptId}", appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate invoice for appointment {ApptId} - manual intervention required", appointmentId);
            }
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                if (patient != null && doctor != null)
                {
                    await _realTimeNotificationService.NotifyAppointmentCompleted(patient.User.Id, appointment.Id, $"{doctor.User.FirstName} {doctor.User.LastName}", diagnosis);
                    await _emailNotificationService.SendAppointmentCompletedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{doctor.User.FirstName} {doctor.User.LastName}", diagnosis, prescription);
                }
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send notifications");
            }
            _logger.LogInformation($"Appointment {appointmentId} completed by doctor {doctorId}");
            await NotifyDashboardUpdateAsync();
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing appointment {appointmentId}");
            return null;
        }
    }
    // cancel appointment
    public async Task<bool> CancelAppointmentAsync(int appointmentId, int userId, string cancelledBy, string? reason = null)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
            bool hasAccess = (patient != null && patient.User.Id == userId) || (doctor != null && doctor.User.Id == userId);
            if (!hasAccess)
                return false;
            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return false;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            appointment.Status = AppointmentStatus.Cancelled;
            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = cancelledBy,
                ChangeReason = reason ?? "Cancelled"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                if (patient != null && doctor != null)
                {
                    await _realTimeNotificationService.NotifyAppointmentCancelled(doctor.User.Id, patient.User.Id, appointment.Id, reason);
                }
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send cancellation notifications");
            }
            _logger.LogInformation($"Appointment {appointmentId} cancelled by {cancelledBy}");
            await NotifyDashboardUpdateAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling appointment {appointmentId}");
            return false;
        }
    }

    // reschedule appointment
    public async Task<AppointmentResponseDto?> RescheduleAppointmentAsync(int appointmentId, int userId, DateTime newDate, TimeSpan newStartTime, TimeSpan newEndTime)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
            bool hasAccess = (patient != null && patient.User.Id == userId) || (doctor != null && doctor.User.Id == userId);
            if (!hasAccess)
                return null;
            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
                return null;
            bool hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
                appointment.DoctorId, newDate, newStartTime, newEndTime, appointmentId);
            if (hasConflict)
                return null;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            var oldDate = appointment.AppointmentDate;
            appointment.AppointmentDate = newDate;
            appointment.StartTime = newStartTime;
            appointment.EndTime = newEndTime;
            appointment.IsRescheduled = true;
            appointment.RescheduledAt = DateTime.UtcNow;
            appointment.OriginalAppointmentId = appointment.OriginalAppointmentId ?? appointment.Id;

            if (appointment.ApprovalStatus == AppointmentApprovalStatus.Approved)
            {
                appointment.ApprovalStatus = AppointmentApprovalStatus.Pending;
                appointment.ApprovedByDoctorId = null;
                appointment.ApprovedAt = null;
            }

            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = $"UserId:{userId}",
                ChangeReason = "Appointment Rescheduled"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            try
            {
                if (doctor != null)
                {
                    await _realTimeNotificationService.NotifyAppointmentRescheduled(doctor.User.Id, appointment.Id, oldDate, newDate);
                }
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to send reschedule notifications");
            }
            _logger.LogInformation($"Appointment {appointmentId} rescheduled");
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rescheduling appointment {appointmentId}");
            return null;
        }
    }

    // mark as no-show
    public async Task<bool> MarkAsNoShowAsync(int appointmentId)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;
            var prevStatus = appointment.Status;
            var prevApprovalStatus = appointment.ApprovalStatus;
            appointment.Status = AppointmentStatus.NoShow;
            _unitOfWork.Appointments.Update(appointment);
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = prevStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = prevApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = "System",
                ChangeReason = "Marked as No-Show"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking appointment {appointmentId} as no-show");
            return false;
        }
    }

    // get pending approvals
    public async Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int doctorId)
    {
        try
        {
            var pendingAppointments = await _unitOfWork.Appointments.GetPendingApprovalsAsync(doctorId);
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(pendingAppointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting pending approvals for doctor {doctorId}");
            return new List<AppointmentResponseDto>();
        }
    }

    // get status history
    public async Task<IEnumerable<AppointmentStatusHistoryDto>> GetStatusHistoryAsync(int appointmentId)
    {
        try
        {
            var histories = await _unitOfWork.AppointmentStatusHistories.GetByAppointmentIdAsync(appointmentId);
            return histories.Select(h => new AppointmentStatusHistoryDto
            {
                Id = h.Id,
                PreviousStatus = h.PreviousStatus?.ToString(),
                NewStatus = h.NewStatus.ToString(),
                PreviousApprovalStatus = h.PreviousApprovalStatus?.ToString(),
                NewApprovalStatus = h.NewApprovalStatus?.ToString(),
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt,
                ChangeReason = h.ChangeReason
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting status history for appointment {appointmentId}");
            return new List<AppointmentStatusHistoryDto>();
        }
    }

    // get available slots
    public async Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        try
        {
            var slots = await _unitOfWork.Appointments.GetAvailableSlotsAsync(doctorId, date);
            return slots.Select(slot => new TimeSlotDto { StartTime = slot, EndTime = slot.Add(TimeSpan.FromMinutes(30)), IsAvailable = true }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting available slots for doctor {doctorId}");
            return new List<TimeSlotDto>();
        }
    }

    // validate doctor working hours
    private async Task<bool> ValidateDoctorWorkingHoursAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
    {
        try
        {
            var dayOfWeek = (int)appointmentDate.DayOfWeek;
            var daySchedule = await _unitOfWork.DoctorWorkingHours.GetByDoctorIdAndDayAsync(doctorId, dayOfWeek);
            if (daySchedule != null)
            {
                if (!daySchedule.IsWorkingDay)
                    return false;
                return startTime >= daySchedule.StartTime && endTime <= daySchedule.EndTime;
            }
            var defaultStart = new TimeSpan(9, 0, 0);
            var defaultEnd = new TimeSpan(21, 0, 0);
            return startTime >= defaultStart && endTime <= defaultEnd;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating doctor working hours for doctor {doctorId}");
            return false;
        }
    }

    // notify dashboard update
    private async Task NotifyDashboardUpdateAsync()
    {
        try
        {
            var stats = await _reportingService.GetSystemStatisticsAsync();
            await _realTimeNotificationService.BroadcastDashboardUpdate(stats.TotalAppointments, stats.AppointmentsToday, stats.PendingApprovals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast dashboard update");
        }
    }
}