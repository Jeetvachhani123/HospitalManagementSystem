using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/doctors/{doctorId}/working-hours")]
[Authorize]
public class WorkingHoursController : ControllerBase
{
    private readonly IWorkingHoursService _workingHoursService;
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<WorkingHoursController> _logger;
    public WorkingHoursController(IWorkingHoursService workingHoursService, IAppointmentService appointmentService, ILogger<WorkingHoursController> logger)
    {
        _workingHoursService = workingHoursService;
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<WorkingHoursDto>>>> GetWorkingHours(int doctorId)
    {
        var workingHours = await _workingHoursService.GetWorkingHoursAsync(doctorId);
        return Ok(ApiResponse<List<WorkingHoursDto>>.SuccessResponse(workingHours));
    }

    [HttpPut]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateWorkingHours(int doctorId, [FromBody] List<WorkingHoursDto> workingHours)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Doctor")
        {
            var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
            if (doctor == null || doctor.Id != doctorId)
            {
                _logger.LogWarning("Doctor {UserId} attempted to update working hours for doctor {DoctorId}", userId, doctorId);
                return Forbid();
            }
        }

        if (workingHours == null || !workingHours.Any())
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Working hours cannot be empty"));
        }
        foreach (var hours in workingHours)
        {
            if (hours.StartTime >= hours.EndTime)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse($"Invalid time range for {hours.DayOfWeek}: Start time must be before end time"));
            }
        }

        await _workingHoursService.UpdateWorkingHoursAsync(doctorId, workingHours);
        _logger.LogInformation("Working hours updated for doctor {DoctorId}", doctorId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Working hours updated successfully"));
    }

    [HttpGet("~/api/my-working-hours")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<WorkingHoursDto>>>> GetMyWorkingHours()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<List<WorkingHoursDto>>.ErrorResponse("Doctor profile not found"));
        }

        var workingHours = await _workingHoursService.GetWorkingHoursAsync(doctor.Id);
        return Ok(ApiResponse<List<WorkingHoursDto>>.SuccessResponse(workingHours));
    }

    [HttpPut("~/api/my-working-hours")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMyWorkingHours([FromBody] List<WorkingHoursDto> workingHours)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Doctor profile not found"));
        }

        if (workingHours == null || !workingHours.Any())
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Working hours cannot be empty"));
        }
        foreach (var hours in workingHours)
        {
            if (hours.StartTime >= hours.EndTime)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse($"Invalid time range for {hours.DayOfWeek}: Start time must be before end time"));
            }
        }

        await _workingHoursService.UpdateWorkingHoursAsync(doctor.Id, workingHours);
        _logger.LogInformation("Working hours updated for doctor {DoctorId}", doctor.Id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Working hours updated successfully"));
    }
}