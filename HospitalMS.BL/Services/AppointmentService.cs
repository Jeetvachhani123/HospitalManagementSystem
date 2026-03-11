using AutoMapper;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.DTOs.Patient;
using HospitalMS.BL.Exceptions;
using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;
    public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // get appointment by id
    public async Task<AppointmentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdForReadAsync(id, cancellationToken);
        return appointment == null ? null : MapToAppointmentResponse(appointment);
    }

    // get all appointments
    public async Task<IEnumerable<AppointmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get by patient id
    public async Task<IEnumerable<AppointmentResponseDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetByPatientIdAsync(patientId, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get by doctor id
    public async Task<IEnumerable<AppointmentResponseDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetByDoctorIdAsync(doctorId, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get by date range
    public async Task<IEnumerable<AppointmentResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
            throw new ValidationException("Start date must be before or equal to end date");
        var appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get by status
    public async Task<IEnumerable<AppointmentResponseDto>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetByStatusAsync(status, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // create new appointment
    public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto appointmentDto, CancellationToken cancellationToken = default)
    {
        var fullDateTime = EnsureUtc(appointmentDto.AppointmentDate.Date + appointmentDto.StartTime);
        if (fullDateTime <= DateTime.UtcNow)
            throw new BusinessRuleException("PastAppointment", "Cannot create appointments in the past");
        if (appointmentDto.StartTime >= appointmentDto.EndTime)
            throw new ValidationException("Start time must be before end time");
        var patient = await _unitOfWork.Patients.GetByIdAsync(appointmentDto.PatientId, cancellationToken);
        if (patient == null)
        {
            throw new NotFoundException("Patient", appointmentDto.PatientId);
        }
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointmentDto.DoctorId, cancellationToken);
        if (doctor == null)
        {
            throw new NotFoundException("Doctor", appointmentDto.DoctorId);
        }
        var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(appointmentDto.DoctorId, appointmentDto.AppointmentDate, appointmentDto.StartTime, appointmentDto.EndTime, cancellationToken: cancellationToken);
        if (hasConflict)
        {
            throw new ConflictException($"The doctor already has an appointment scheduled during this time slot " + $"({appointmentDto.AppointmentDate:yyyy-MM-dd} {appointmentDto.StartTime} - {appointmentDto.EndTime})");
        }
        var appointment = new Appointment { PatientId = appointmentDto.PatientId, DoctorId = appointmentDto.DoctorId, AppointmentDate = appointmentDto.AppointmentDate, StartTime = appointmentDto.StartTime, EndTime = appointmentDto.EndTime, Reason = appointmentDto.Reason, Status = AppointmentStatus.Scheduled, ApprovalStatus = AppointmentApprovalStatus.Pending };
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var history = new AppointmentStatusHistory
                {
                    AppointmentId = appointment.Id,
                    NewStatus = appointment.Status,
                    NewApprovalStatus = appointment.ApprovalStatus,
                    ChangedBy = $"PatientId:{appointmentDto.PatientId}",
                    ChangeReason = "Appointment Created"
                };
                await _unitOfWork.AppointmentStatusHistories.AddAsync(history, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            throw new ConflictException("SchedulingConflict", "The selected time slot was just booked by another user. Please choose a different time.");
        }
        appointment = await _unitOfWork.Appointments.GetByIdAsync(appointment.Id, cancellationToken) ?? throw new NotFoundException("Appointment", appointment.Id);
        return MapToAppointmentResponse(appointment);
    }

    // update appointment status
    public async Task<AppointmentResponseDto?> UpdateStatusAsync(int id, AppointmentStatusDto statusDto, int? currentDoctorId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
        {
            return null;
        }
        if (statusDto.RowVersion != null && appointment.RowVersion != null)
        {
            if (!statusDto.RowVersion.SequenceEqual(appointment.RowVersion))
            {
                throw new ConcurrencyException("Appointment", id);
            }
        }
        ValidateStatusTransition(appointment.Status, statusDto.Status);
        if ((statusDto.Diagnosis != null || statusDto.Prescription != null) && currentDoctorId.HasValue)
        {
            if (appointment.DoctorId != currentDoctorId.Value)
            {
                throw new UnauthorizedException("Only the assigned doctor can add or modify diagnosis and prescription");
            }
        }
        var previousStatus = appointment.Status;
        appointment.Status = statusDto.Status;
        if (statusDto.Notes != null)
            appointment.Notes = statusDto.Notes;
        if (statusDto.Diagnosis != null)
            appointment.Diagnosis = statusDto.Diagnosis;
        if (statusDto.Prescription != null)
            appointment.Prescription = statusDto.Prescription;
        _unitOfWork.Appointments.Update(appointment);
        if (previousStatus != appointment.Status)
        {
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatus = previousStatus,
                NewStatus = appointment.Status,
                PreviousApprovalStatus = appointment.ApprovalStatus,
                NewApprovalStatus = appointment.ApprovalStatus,
                ChangedBy = currentDoctorId?.ToString() ?? "System",
                ChangeReason = statusDto.Notes != null ? "Status Updated - Note added" : "Status Updated"
            };
            await _unitOfWork.AppointmentStatusHistories.AddAsync(history, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToAppointmentResponse(appointment);
    }

    // update appointment details
    public async Task<AppointmentResponseDto?> UpdateAsync(int id, AppointmentUpdateDto appointmentDto, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            throw new NotFoundException("Appointment", id);
        bool dateTimeChanged = appointmentDto.AppointmentDate.HasValue || appointmentDto.StartTime.HasValue || appointmentDto.EndTime.HasValue;
        if (dateTimeChanged)
        {
            var newDate = appointmentDto.AppointmentDate ?? appointment.AppointmentDate;
            var newStartTime = appointmentDto.StartTime ?? appointment.StartTime;
            var newEndTime = appointmentDto.EndTime ?? appointment.EndTime;
            var newFullDateTime = EnsureUtc(newDate.Date + newStartTime);
            if (newFullDateTime <= DateTime.UtcNow)
                throw new BusinessRuleException("PastAppointment", "Cannot schedule appointments in the past");
            if (newStartTime >= newEndTime)
                throw new ValidationException("Start time must be before end time");
            var hasConflict = await _unitOfWork.Appointments.HasConflictAsync(
                appointment.DoctorId,
                newDate,
                newStartTime,
                newEndTime,
                excludeAppointmentId: id,
                cancellationToken: cancellationToken
            );
            if (hasConflict)
            {
                throw new ConflictException($"The doctor already has an appointment scheduled during this time slot " + $"({newDate:yyyy-MM-dd} {newStartTime} - {newEndTime})");
            }

            appointment.IsRescheduled = true;
            appointment.RescheduledAt = DateTime.UtcNow;
            appointment.OriginalAppointmentId = appointment.OriginalAppointmentId ?? appointment.Id;
        }
        if (appointmentDto.RowVersion != null)
            appointment.RowVersion = appointmentDto.RowVersion;
        if (appointmentDto.AppointmentDate.HasValue)
            appointment.AppointmentDate = appointmentDto.AppointmentDate.Value;
        if (appointmentDto.StartTime.HasValue)
            appointment.StartTime = appointmentDto.StartTime.Value;
        if (appointmentDto.EndTime.HasValue)
            appointment.EndTime = appointmentDto.EndTime.Value;
        if (appointmentDto.Reason != null)
            appointment.Reason = appointmentDto.Reason;
        if (appointmentDto.Notes != null)
            appointment.Notes = appointmentDto.Notes;
        if (appointmentDto.Diagnosis != null)
            appointment.Diagnosis = appointmentDto.Diagnosis;
        if (appointmentDto.Prescription != null)
            appointment.Prescription = appointmentDto.Prescription;
        try
        {
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
            return appointment == null ? null : MapToAppointmentResponse(appointment);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            _logger.LogWarning(ex, "Concurrency conflict for appointment {AppointmentId}", id);
            throw new ConcurrencyException("Appointment", id);
        }
    }

    // cancel appointment
    public async Task<bool> CancelAsync(int id, string? cancellationReason = null, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment", id);
        }
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw new BusinessRuleException("AlreadyCancelled", "This appointment has already been cancelled");
        }
        if (appointment.Status == AppointmentStatus.Completed)
        {
            throw new BusinessRuleException("CannotCancelCompleted", "Cannot cancel a completed appointment");
        }
        var appointmentDateTime = EnsureUtc(appointment.GetFullStartDateTime());
        if (appointmentDateTime < DateTime.UtcNow)
        {
            throw new BusinessRuleException("PastAppointment", "Cannot cancel past appointments");
        }
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;
        if (hoursUntilAppointment < 24)
        {
            throw new BusinessRuleException("TooCloseToAppointment", $"Cannot cancel appointment within 24 hours of scheduled time. " + $"Appointment is in {hoursUntilAppointment:F1} hours.");
        }
        var previousStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(cancellationReason))
        {
            appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes) ? $"Cancelled: {cancellationReason}" : $"{appointment.Notes}\nCancelled: {cancellationReason}";
        }
        _unitOfWork.Appointments.Update(appointment);
        var history = new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            PreviousStatus = previousStatus,
            NewStatus = appointment.Status,
            PreviousApprovalStatus = appointment.ApprovalStatus,
            NewApprovalStatus = appointment.ApprovalStatus,
            ChangedBy = "System/User",
            ChangeReason = cancellationReason ?? "Cancelled by user"
        };
        await _unitOfWork.AppointmentStatusHistories.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // check slot conflict
    public async Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        if (startTime >= endTime)
            throw new ValidationException("Start time must be before end time");
        return await _unitOfWork.Appointments.HasConflictAsync(doctorId, appointmentDate, startTime, endTime, excludeAppointmentId, cancellationToken);
    }

    // approve appointment
    public async Task<AppointmentResponseDto> ApproveAsync(int id, int doctorId, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            throw new NotFoundException("Appointment", id);
        if (appointment.DoctorId != doctorId)
            throw new UnauthorizedException("Only the assigned doctor can approve this appointment");
        if (appointment.ApprovalStatus == AppointmentApprovalStatus.Approved)
            throw new BusinessRuleException("AlreadyApproved", "This appointment has already been approved");
        if (appointment.ApprovalStatus == AppointmentApprovalStatus.Rejected)
            throw new BusinessRuleException("AlreadyRejected", "This appointment has been rejected and cannot be approved");
        var previousApprovalStatus = appointment.ApprovalStatus;
        appointment.ApprovalStatus = AppointmentApprovalStatus.Approved;
        appointment.ApprovedByDoctorId = doctorId;
        appointment.ApprovedAt = DateTime.UtcNow;
        appointment.RejectionReason = null;
        _unitOfWork.Appointments.Update(appointment);
        var history = new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            PreviousStatus = appointment.Status,
            NewStatus = appointment.Status,
            PreviousApprovalStatus = previousApprovalStatus,
            NewApprovalStatus = appointment.ApprovalStatus,
            ChangedBy = $"DoctorId:{doctorId}",
            ChangeReason = "Approved by doctor"
        };
        await _unitOfWork.AppointmentStatusHistories.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToAppointmentResponse(appointment);
    }

    // reject appointment
    public async Task<AppointmentResponseDto> RejectAsync(int id, int doctorId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ValidationException("Rejection reason is required");
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            throw new NotFoundException("Appointment", id);
        if (appointment.DoctorId != doctorId)
            throw new UnauthorizedException("Only the assigned doctor can reject this appointment");
        if (appointment.ApprovalStatus == AppointmentApprovalStatus.Rejected)
            throw new BusinessRuleException("AlreadyRejected", "This appointment has already been rejected");
        if (appointment.ApprovalStatus == AppointmentApprovalStatus.Approved)
            throw new BusinessRuleException("AlreadyApproved", "This appointment has been approved and cannot be rejected");
        var previousApprovalStatus = appointment.ApprovalStatus;
        var previousStatus = appointment.Status;
        appointment.ApprovalStatus = AppointmentApprovalStatus.Rejected;
        appointment.RejectionReason = rejectionReason;
        appointment.Status = AppointmentStatus.Cancelled;
        _unitOfWork.Appointments.Update(appointment);
        var history = new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            PreviousStatus = previousStatus,
            NewStatus = appointment.Status,
            PreviousApprovalStatus = previousApprovalStatus,
            NewApprovalStatus = appointment.ApprovalStatus,
            ChangedBy = $"DoctorId:{doctorId}",
            ChangeReason = "Rejected: " + rejectionReason
        };
        await _unitOfWork.AppointmentStatusHistories.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToAppointmentResponse(appointment);
    }

    // get pending approvals
    public async Task<IEnumerable<AppointmentResponseDto>> GetPendingApprovalsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetPendingApprovalsAsync(doctorId, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get today's appointments
    public async Task<IEnumerable<AppointmentResponseDto>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetTodaysAppointmentsAsync(cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // get recent appointments
    public async Task<IEnumerable<AppointmentResponseDto>> GetRecentAppointmentsAsync(int count, CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetRecentAsync(count, cancellationToken);
        return appointments.Select(MapToAppointmentResponse);
    }

    // validate status transition
    private void ValidateStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        if (currentStatus == newStatus)
            return;
        var invalidTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            { AppointmentStatus.Cancelled, new[] { AppointmentStatus.Scheduled, AppointmentStatus.InProgress, AppointmentStatus.Completed } },
            { AppointmentStatus.Completed, new[] { AppointmentStatus.Scheduled, AppointmentStatus.InProgress, AppointmentStatus.Cancelled } },
            { AppointmentStatus.NoShow, new[] { AppointmentStatus.Scheduled, AppointmentStatus.InProgress, AppointmentStatus.Completed } }
        };
        if (invalidTransitions.TryGetValue(currentStatus, out var forbidden) && forbidden.Contains(newStatus))
        {
            throw new BusinessRuleException("InvalidStatusTransition", $"Cannot transition from {currentStatus} to {newStatus}");
        }
    }

    // ensure utc datetime
    private DateTime EnsureUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;
        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    // map to response dto
    private AppointmentResponseDto MapToAppointmentResponse(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.User?.GetFullName() ?? "Unknown Patient",
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor?.User?.GetFullName() ?? "Unknown Doctor",
            DoctorSpecialization = appointment.Doctor?.Specialization ?? "Unknown",
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            StatusEnum = appointment.Status,
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            Diagnosis = appointment.Diagnosis,
            Prescription = appointment.Prescription,
            CreatedAt = appointment.CreatedAt,
            ApprovalStatus = appointment.ApprovalStatus.ToString(),
            ApprovalStatusEnum = appointment.ApprovalStatus,
            CreatedBy = appointment.CreatedBy,
            UpdatedBy = appointment.UpdatedBy
        };
    }

    // check user appointment access
    public async Task<bool> UserHasAccessToAppointmentAsync(int userId, int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            return false;
        var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId, cancellationToken);
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointment.DoctorId, cancellationToken);
        return (patient != null && patient.User.Id == userId) || (doctor != null && doctor.User.Id == userId);
    }

    // get doctor by user id
    public async Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId, cancellationToken);
        if (doctor == null)
            return null;
        return new DoctorResponseDto { Id = doctor.Id, UserId = doctor.User.Id, FullName = doctor.User.GetFullName(), Email = doctor.User.Email, Specialization = doctor.Specialization, LicenseNumber = doctor.LicenseNumber, ConsultationFee = doctor.ConsultationFee, IsAvailable = doctor.IsAvailable };
    }

    // get patient by user id
    public async Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var patient = await _unitOfWork.Patients.GetByUserIdAsync(userId, cancellationToken);
        if (patient == null)
            return null;
        return new PatientResponseDto { Id = patient.Id, UserId = patient.User.Id, FullName = patient.User.GetFullName(), Email = patient.User.Email, PhoneNumber = patient.User.PhoneNumber, DateOfBirth = patient.DateOfBirth, Gender = patient.Gender, BloodGroup = patient.BloodGroup };
    }

    public Task<int> GetAppointmentsCountAsync(int doctorId, AppointmentStatus? status = null, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.Appointments.CountAsync(a =>
            a.DoctorId == doctorId &&
            (!status.HasValue || a.Status == status.Value) &&
            (!date.HasValue || a.AppointmentDate.Date == date.Value.Date),
            cancellationToken);
    }

    public async Task<(IEnumerable<AppointmentResponseDto> Items, int TotalCount)> SearchAsync(string? searchTerm, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Appointments.SearchAsync(searchTerm, doctorId, patientId, fromDate, toDate, status, page, pageSize, cancellationToken);
        return (items.Select(MapToAppointmentResponse), totalCount);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingByDoctorIdAsync(int doctorId, int count, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var (items, _) = await _unitOfWork.Appointments.SearchAsync(null, doctorId, null, today, null, AppointmentStatus.Scheduled, 1, count, cancellationToken);
        return items.Select(MapToAppointmentResponse);
    }
}