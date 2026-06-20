using AutoMapper;
using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.DATA.UnitOfWork;
using HospitalMS.Models;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public interface IAppointmentWorkflowService
{
    Task<AppointmentResponseDto?> RequestAppointmentAsync(AppointmentCreateDto dto, int userId);
    Task<AppointmentResponseDto?> ApproveAppointmentAsync(int appointmentId, int userId);
    Task<AppointmentResponseDto?> RejectAppointmentAsync(int appointmentId, int userId, string rejectionReason);
    Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, int userId, string? diagnosis, string? prescription, string? notes);
    Task<bool> CancelAppointmentAsync(int appointmentId, int userId, string cancelledBy, string? reason = null);
    Task<AppointmentResponseDto?> RescheduleAppointmentAsync(int appointmentId, int userId, DateTime newDate, TimeSpan newStartTime, TimeSpan newEndTime);
    Task<bool> MarkAsNoShowAsync(int appointmentId);
    Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int userId);
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

    public async Task<AppointmentResponseDto?> RequestAppointmentAsync(AppointmentCreateDto dto, int userId)
    {
        try
        {
            if (dto.AppointmentDate.Date < DateTime.UtcNow.Date)
                return null;

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
                return null;

            var patient = await _unitOfWork.Patients.GetByUserIdAsync(userId);
            if (patient == null)
                return null;

            bool hasConflict = await _unitOfWork.Appointments.HasConflictAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime);
            if (hasConflict)
                return null;

            if (!await ValidateDoctorWorkingHoursAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime))
                return null;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var appointment = new Appointment { PatientId = patient.Id, DoctorId = dto.DoctorId, AppointmentDate = dto.AppointmentDate, StartTime = dto.StartTime, EndTime = dto.EndTime, Reason = dto.Reason, Status = AppointmentStatus.Scheduled, ApprovalStatus = AppointmentApprovalStatus.Pending, CreatedAt = DateTime.UtcNow };
                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                var history = new AppointmentStatusHistory
                {
                    AppointmentId = appointment.Id,
                    NewStatus = appointment.Status,
                    NewApprovalStatus = appointment.ApprovalStatus,
                    ChangedBy = $"UserId:{userId}",
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
                _logger.LogInformation("Appointment {AppointmentId} requested by userId {UserId}", appointment.Id, userId);
                await NotifyDashboardUpdateAsync();
                var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == dto.DoctorId && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
                await _realTimeNotificationService.UpdatePendingCount(doctor.UserId, pendingCount);
                return _mapper.Map<AppointmentResponseDto>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting appointment");
            throw;
        }
    }

    public async Task<AppointmentResponseDto?> ApproveAppointmentAsync(int appointmentId, int userId)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;

            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return null;

            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Pending)
                return null;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var prevStatus = appointment.Status;
                var prevApprovalStatus = appointment.ApprovalStatus;
                appointment.ApprovalStatus = AppointmentApprovalStatus.Approved;
                appointment.ApprovedByDoctorId = doctor.Id;
                appointment.ApprovedAt = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);
                var history = new AppointmentStatusHistory
                {
                    AppointmentId = appointment.Id,
                    PreviousStatus = prevStatus,
                    NewStatus = appointment.Status,
                    PreviousApprovalStatus = prevApprovalStatus,
                    NewApprovalStatus = appointment.ApprovalStatus,
                    ChangedBy = $"DoctorId:{doctor.Id}",
                    ChangeReason = "Appointment Approved"
                };
                await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                try
                {
                    var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                    var apptDoctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                    if (patient != null && apptDoctor != null)
                    {
                        await _realTimeNotificationService.NotifyAppointmentApproved(patient.User.Id, appointment.Id, $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", appointment.GetFullStartDateTime());
                        await _emailNotificationService.SendAppointmentApprovedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", appointment.AppointmentDate, appointment.StartTime);
                    }
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "Failed to send notifications");
                }
                _logger.LogInformation("Appointment {AppointmentId} approved by doctor userId {UserId}", appointmentId, userId);
                await NotifyDashboardUpdateAsync();
                var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == doctor.Id && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
                await _realTimeNotificationService.UpdatePendingCount(appointment.Doctor.UserId, pendingCount);
                return _mapper.Map<AppointmentResponseDto>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<AppointmentResponseDto?> RejectAppointmentAsync(int appointmentId, int userId, string rejectionReason)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;

            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return null;

            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Pending)
                return null;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var prevStatus = appointment.Status;
                var prevApprovalStatus = appointment.ApprovalStatus;
                appointment.ApprovalStatus = AppointmentApprovalStatus.Rejected;
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.RejectionReason = rejectionReason;
                appointment.ApprovedByDoctorId = doctor.Id;
                appointment.ApprovedAt = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);
                var history = new AppointmentStatusHistory
                {
                    AppointmentId = appointment.Id,
                    PreviousStatus = prevStatus,
                    NewStatus = appointment.Status,
                    PreviousApprovalStatus = prevApprovalStatus,
                    NewApprovalStatus = appointment.ApprovalStatus,
                    ChangedBy = $"DoctorId:{doctor.Id}",
                    ChangeReason = "Rejected: " + rejectionReason
                };
                await _unitOfWork.AppointmentStatusHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                try
                {
                    var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                    var apptDoctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                    if (patient != null && apptDoctor != null)
                    {
                        await _realTimeNotificationService.NotifyAppointmentRejected(patient.User.Id, appointment.Id, $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", rejectionReason);
                        await _emailNotificationService.SendAppointmentRejectedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", rejectionReason);
                    }
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "Failed to send notifications");
                }
                _logger.LogInformation("Appointment {AppointmentId} rejected by doctor userId {UserId}", appointmentId, userId);
                await NotifyDashboardUpdateAsync();
                var pendingCount = await _unitOfWork.Appointments.CountAsync(a => a.DoctorId == doctor.Id && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
                await _realTimeNotificationService.UpdatePendingCount(appointment.Doctor.UserId, pendingCount);
                return _mapper.Map<AppointmentResponseDto>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<AppointmentResponseDto?> CompleteAppointmentAsync(int appointmentId, int userId, string? diagnosis, string? prescription, string? notes)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return null;

            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return null;

            if (appointment.Status == AppointmentStatus.Completed)
                return null;

            if (appointment.ApprovalStatus != AppointmentApprovalStatus.Approved)
                return null;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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
                    ChangedBy = $"DoctorId:{doctor.Id}",
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
                    var apptDoctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId);
                    if (patient != null && apptDoctor != null)
                    {
                        await _realTimeNotificationService.NotifyAppointmentCompleted(patient.User.Id, appointment.Id, $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", diagnosis);
                        await _emailNotificationService.SendAppointmentCompletedEmailAsync(patient.User.Email, $"{patient.User.FirstName} {patient.User.LastName}", $"{apptDoctor.User.FirstName} {apptDoctor.User.LastName}", diagnosis, prescription);
                    }
                }
                catch (Exception notifEx)
                {
                    _logger.LogWarning(notifEx, "Failed to send notifications");
                }
                _logger.LogInformation("Appointment {AppointmentId} completed by doctor userId {UserId}", appointmentId, userId);
                await NotifyDashboardUpdateAsync();

                return _mapper.Map<AppointmentResponseDto>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

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
            _logger.LogInformation("Appointment {AppointmentId} cancelled by {CancelledBy}", appointmentId, cancelledBy);
            await NotifyDashboardUpdateAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

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

            bool hasConflict = await _unitOfWork.Appointments.HasConflictAsync(appointment.DoctorId, newDate, newStartTime, newEndTime, appointmentId);
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
            _logger.LogInformation("Appointment {AppointmentId} rescheduled", appointmentId);
            return _mapper.Map<AppointmentResponseDto>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

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
            _logger.LogError(ex, "Error marking appointment {AppointmentId} as no-show", appointmentId);
            throw;
        }
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int userId)
    {
        try
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId);
            if (doctor == null)
                return new List<AppointmentResponseDto>();

            var pendingAppointments = await _unitOfWork.Appointments.GetPendingApprovalsAsync(doctor.Id);
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(pendingAppointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals for userId {UserId}", userId);
            throw;
        }
    }

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
            _logger.LogError(ex, "Error getting status history for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        try
        {
            var slots = await _unitOfWork.Appointments.GetAvailableSlotsAsync(doctorId, date);
            return slots.Select(slot => new TimeSlotDto { StartTime = slot, EndTime = slot.Add(TimeSpan.FromMinutes(30)), IsAvailable = true }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available slots for doctor {DoctorId}", doctorId);
            throw;
        }
    }

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
            _logger.LogError(ex, "Error validating doctor working hours for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    private async Task NotifyDashboardUpdateAsync()
    {
        try
        {
            var totalAppointments = await _unitOfWork.Appointments.CountAsync();
            var appointmentsToday = await _unitOfWork.Appointments.CountAsync(a => a.AppointmentDate >= DateTime.Today && a.AppointmentDate < DateTime.Today.AddDays(1));
            var pendingApprovals = await _unitOfWork.Appointments.CountAsync(a => a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled);
            await _realTimeNotificationService.BroadcastDashboardUpdate(totalAppointments, appointmentsToday, pendingApprovals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast dashboard update");
        }
    }
}