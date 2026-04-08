using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces.Services;
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
        try
        {
            var patientId = GetPatientIdFromClaims();
            var result = await _workflowService.RequestAppointmentAsync(dto, patientId);
            if (result == null)
            {
                _logger.LogWarning($"Patient {patientId} appointment request failed");
                return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to request appointment. Please verify doctor availability and time slot."));
            }

            _logger.LogInformation($"Appointment {result.Id} requested by patient {patientId}");
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment requested successfully. Awaiting doctor approval."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting appointment");
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("An error occurred while processing your request."));
        }
    }

    [HttpPost("{appointmentId}/approve")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> ApproveAppointment(int appointmentId)
    {
        try
        {
            var doctorId = GetDoctorIdFromClaims();
            var result = await _workflowService.ApproveAppointmentAsync(appointmentId, doctorId);
            if (result == null)
            {
                _logger.LogWarning($"Doctor {doctorId} failed to approve appointment {appointmentId}");
                return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to approve appointment. It may not be pending or you may not be the assigned doctor."));
            }

            _logger.LogInformation($"Appointment {appointmentId} approved by doctor {doctorId}");
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment approved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving appointment {appointmentId}");
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("An error occurred while approving the appointment."));
        }
    }

    [HttpPost("{appointmentId}/reject")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> RejectAppointment(int appointmentId, [FromBody] RejectAppointmentDto dto)
    {
        try
        {
            var doctorId = GetDoctorIdFromClaims();
            var result = await _workflowService.RejectAppointmentAsync(appointmentId, doctorId, dto.RejectionReason);
            if (result == null)
            {
                _logger.LogWarning($"Doctor {doctorId} failed to reject appointment {appointmentId}");
                return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to reject appointment. It may not be pending or you may not be the assigned doctor."));
            }

            _logger.LogInformation($"Appointment {appointmentId} rejected by doctor {doctorId}");
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment rejected. Patient will be notified of the reason."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting appointment {appointmentId}");
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("An error occurred while rejecting the appointment."));
        }
    }

    [HttpPost("{appointmentId}/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> CompleteAppointment(int appointmentId, [FromBody] CompleteAppointmentDto dto)
    {
        try
        {
            var doctorId = GetDoctorIdFromClaims();
            var result = await _workflowService.CompleteAppointmentAsync(appointmentId, doctorId, dto.Diagnosis, dto.Prescription, dto.Notes);
            if (result == null)
            {
                _logger.LogWarning($"Doctor {doctorId} failed to complete appointment {appointmentId}");
                return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to complete appointment. It may not be in approved state or you may not be the assigned doctor."));
            }

            _logger.LogInformation($"Appointment {appointmentId} marked as completed by doctor {doctorId}");
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment completed successfully with consultation notes saved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing appointment {appointmentId}");
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("An error occurred while completing the appointment."));
        }
    }

    [HttpPost("{appointmentId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
            var result = await _workflowService.CancelAppointmentAsync(appointmentId, userId, userEmail, dto.CancellationReason);
            if (!result)
            {
                _logger.LogWarning($"Failed to cancel appointment {appointmentId} by user {userId}");
                return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to cancel appointment. It may already be completed, cancelled, or you don't have permission."));
            }

            _logger.LogInformation($"Appointment {appointmentId} cancelled by {userEmail}");
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment cancelled successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling appointment {appointmentId}");
            return BadRequest(ApiResponse<bool>.ErrorResponse("An error occurred while cancelling the appointment."));
        }
    }

    [HttpPost("{appointmentId}/reschedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponseDto>>> RescheduleAppointment(int appointmentId, [FromBody] RescheduleAppointmentDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var result = await _workflowService.RescheduleAppointmentAsync(appointmentId, userId, dto.NewDate, dto.NewStartTime, dto.NewEndTime);
            if (result == null)
            {
                _logger.LogWarning($"Failed to reschedule appointment {appointmentId} by user {userId}");
                return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("Failed to reschedule appointment. The new time slot may be unavailable, appointment may be in final state, or you don't have permission."));
            }

            _logger.LogInformation($"Appointment {appointmentId} rescheduled to {dto.NewDate} {dto.NewStartTime}");
            return Ok(ApiResponse<AppointmentResponseDto>.SuccessResponse(result, "Appointment rescheduled successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rescheduling appointment {appointmentId}");
            return BadRequest(ApiResponse<AppointmentResponseDto>.ErrorResponse("An error occurred while rescheduling the appointment."));
        }
    }

    [HttpPost("{appointmentId}/no-show")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsNoShow(int appointmentId)
    {
        try
        {
            var result = await _workflowService.MarkAsNoShowAsync(appointmentId);
            if (!result)
            {
                _logger.LogWarning($"Failed to mark appointment {appointmentId} as no-show");
                return NotFound(ApiResponse<bool>.ErrorResponse("Appointment not found."));
            }

            _logger.LogInformation($"Appointment {appointmentId} marked as no-show");
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Appointment marked as no-show."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking appointment {appointmentId} as no-show");
            return BadRequest(ApiResponse<bool>.ErrorResponse("An error occurred while marking the appointment as no-show."));
        }
    }

    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentResponseDto>>>> GetPendingApprovals()
    {
        try
        {
            var doctorId = GetDoctorIdFromClaims();
            var result = await _workflowService.GetPendingApprovalsAsync(doctorId);
            _logger.LogInformation($"Retrieved {result.Count()} pending approvals for doctor {doctorId}");
            
            return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approvals");
            return Ok(ApiResponse<IEnumerable<AppointmentResponseDto>>.SuccessResponse(new List<AppointmentResponseDto>(), "No pending approvals."));
        }
    }

    [HttpGet("doctors/{doctorId}/available-slots")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TimeSlotDto>>>> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
    {
        try
        {
            if (date.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(ApiResponse<IEnumerable<TimeSlotDto>>.ErrorResponse("Cannot request slots for past dates."));
            }

            var result = await _workflowService.GetAvailableSlotsAsync(doctorId, date);
            _logger.LogInformation($"Retrieved {result.Count()} available slots for doctor {doctorId} on {date:yyyy-MM-dd}");
            return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving available slots for doctor {doctorId}");
            return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.SuccessResponse(new List<TimeSlotDto>(), "No available slots for the selected date."));
        }
    }

    [HttpGet("{appointmentId}/status-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>>> GetStatusHistory(int appointmentId)
    {
        try
        {
            var result = await _workflowService.GetStatusHistoryAsync(appointmentId);
            if (result == null || !result.Any())
            {
                return NotFound(ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>.ErrorResponse("Appointment not found or has no status history."));
            }

            _logger.LogInformation($"Retrieved status history for appointment {appointmentId}");
            return Ok(ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving status history for appointment {appointmentId}");
            return NotFound(ApiResponse<IEnumerable<AppointmentStatusHistoryDto>>.ErrorResponse("Failed to retrieve status history."));
        }
    }

    private int GetPatientIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim ?? "0");
    }

    private int GetDoctorIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim ?? "0");
    }
}