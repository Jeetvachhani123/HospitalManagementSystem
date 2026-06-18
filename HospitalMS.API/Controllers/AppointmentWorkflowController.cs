using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentWorkflowController : ControllerBase
{
    private readonly IAppointmentWorkflowService _workflowService;
    private readonly ILogger<AppointmentWorkflowController> _logger;
    public AppointmentWorkflowController(IAppointmentWorkflowService workflowService, ILogger<AppointmentWorkflowController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    [HttpPost("request")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> RequestAppointment([FromBody] AppointmentCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.RequestAppointmentAsync(dto, userId.Value);
        if (result == null)
        {
            _logger.LogWarning("Patient userId {UserId} appointment request failed", userId);
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to request appointment. Please verify doctor availability and time slot."));
        }

        _logger.LogInformation("Appointment {AppointmentId} requested by userId {UserId}", result.Id, userId);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment requested successfully. Awaiting doctor approval."));
    }

    [HttpPost("{appointmentId}/approve")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> ApproveAppointment(int appointmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.ApproveAppointmentAsync(appointmentId, userId.Value);
        if (result == null)
        {
            _logger.LogWarning("Doctor userId {UserId} failed to approve appointment {AppointmentId}", userId, appointmentId);
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to approve appointment. It may not be pending or you may not be the assigned doctor."));
        }

        _logger.LogInformation("Appointment {AppointmentId} approved by doctor userId {UserId}", appointmentId, userId);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment approved successfully."));
    }

    [HttpPost("{appointmentId}/reject")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> RejectAppointment(int appointmentId, [FromBody] RejectAppointmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.RejectAppointmentAsync(appointmentId, userId.Value, dto.RejectionReason);
        if (result == null)
        {
            _logger.LogWarning("Doctor userId {UserId} failed to reject appointment {AppointmentId}", userId, appointmentId);
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to reject appointment. It may not be pending or you may not be the assigned doctor."));
        }

        _logger.LogInformation("Appointment {AppointmentId} rejected by doctor userId {UserId}", appointmentId, userId);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment rejected. Patient will be notified of the reason."));
    }

    [HttpPost("{appointmentId}/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> CompleteAppointment(int appointmentId, [FromBody] CompleteAppointmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.CompleteAppointmentAsync(appointmentId, userId.Value, dto.Diagnosis, dto.Prescription, dto.Notes);
        if (result == null)
        {
            _logger.LogWarning("Doctor userId {UserId} failed to complete appointment {AppointmentId}", userId, appointmentId);
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to complete appointment. It may not be in approved state or you may not be the assigned doctor."));
        }

        _logger.LogInformation("Appointment {AppointmentId} marked as completed by doctor userId {UserId}", appointmentId, userId);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment completed successfully with consultation notes saved."));
    }

    [HttpPost("{appointmentId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var result = await _workflowService.CancelAppointmentAsync(appointmentId, userId.Value, userEmail, dto.CancellationReason);
        if (!result)
        {
            _logger.LogWarning("Failed to cancel appointment {AppointmentId} by user {UserId}", appointmentId, userId);
            return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel appointment. It may already be completed, cancelled, or you don't have permission."));
        }

        _logger.LogInformation("Appointment {AppointmentId} cancelled by {UserEmail}", appointmentId, userEmail);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment cancelled successfully."));
    }

    [HttpPost("{appointmentId}/reschedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> RescheduleAppointment(int appointmentId, [FromBody] RescheduleAppointmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.RescheduleAppointmentAsync(appointmentId, userId.Value, dto.NewDate, dto.NewStartTime, dto.NewEndTime);
        if (result == null)
        {
            _logger.LogWarning("Failed to reschedule appointment {AppointmentId} by user {UserId}", appointmentId, userId);
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to reschedule appointment. The new time slot may be unavailable, appointment may be in final state, or you don't have permission."));
        }

        _logger.LogInformation("Appointment {AppointmentId} rescheduled to {NewDate} {NewStartTime}", appointmentId, dto.NewDate, dto.NewStartTime);
        return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment rescheduled successfully."));
    }

    [HttpPost("{appointmentId}/no-show")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsNoShow(int appointmentId)
    {
        var result = await _workflowService.MarkAsNoShowAsync(appointmentId);
        if (!result)
        {
            _logger.LogWarning("Failed to mark appointment {AppointmentId} as no-show", appointmentId);
            return NotFound(ApiResponse<bool>.ErrorResponse("Appointment not found."));
        }

        _logger.LogInformation("Appointment {AppointmentId} marked as no-show", appointmentId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment marked as no-show."));
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetPendingApprovals()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _workflowService.GetPendingApprovalsAsync(userId.Value);
        _logger.LogInformation("Retrieved {Count} pending approvals for doctor userId {UserId}", result.Count(), userId);

        return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("doctors/{doctorId}/available-slots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimeSlotDto>>>> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
    {
        if (date.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(ApiResponse<IEnumerable<TimeSlotDto>>.ErrorResponse("Cannot request slots for past dates."));
        }

        var result = await _workflowService.GetAvailableSlotsAsync(doctorId, date);
        _logger.LogInformation("Retrieved {Count} available slots for doctor {DoctorId} on {Date}", result.Count(), doctorId, date.ToString("yyyy-MM-dd"));
        return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.SuccessResponse(result));
    }

    [HttpGet("{appointmentId}/status-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>>> GetStatusHistory(int appointmentId)
    {
        var result = await _workflowService.GetStatusHistoryAsync(appointmentId);
        if (result == null || !result.Any())
        {
            return NotFound(ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>.ErrorResponse("Appointment not found or has no status history."));
        }

        _logger.LogInformation("Retrieved status history for appointment {AppointmentId}", appointmentId);
        return Ok(ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>.SuccessResponse(result));
    }

    private int? GetCurrentUserId()
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return userId;
        return null;
    }
}