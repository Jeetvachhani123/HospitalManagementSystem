using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Doctor;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    public DoctorsController(IDoctorService doctorService, IAppointmentService appointmentService)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DoctorResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var doctors = await _doctorService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<DoctorResponseDto>>.SuccessResponse(doctors));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DoctorResponseDto>>> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorResponseDto>.ErrorResponse(Constants.Messages.DoctorNotFound));
        }
        return Ok(ApiResponse<DoctorResponseDto>.SuccessResponse(doctor));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<DoctorResponseDto>>> GetByUserId(int userId)
    {
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorResponseDto>.ErrorResponse(Constants.Messages.DoctorNotFound));
        }
        return Ok(ApiResponse<DoctorResponseDto>.SuccessResponse(doctor));
    }

    [HttpGet("{id}/calendar")]
    public async Task<ActionResult<ApiResponse<object>>> GetCalendar(int id, [FromQuery] int? month, [FromQuery] int? year)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound(ApiResponse<object>.ErrorResponse(Constants.Messages.DoctorNotFound));
        var targetMonth = month ?? DateTime.Today.Month;
        var targetYear = year ?? DateTime.Today.Year;
        var startDate = new DateTime(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var appointments = await _appointmentService.GetByDoctorIdAsync(id);
        var filteredAppointments = appointments.Where(a => a.AppointmentDate.Date >= startDate && a.AppointmentDate.Date <= endDate);
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Month = targetMonth,
            Year = targetYear,
            Appointments = filteredAppointments.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime).Select(a => new
            {
                a.Id,
                a.PatientName,
                a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.ApprovalStatus
            })
        }));
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DoctorResponseDto>>>> GetAvailable([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var doctors = await _doctorService.GetAvailableDoctorsAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<DoctorResponseDto>>.SuccessResponse(doctors));
    }

    [HttpGet("specialization/{specialization}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DoctorResponseDto>>>> GetBySpecialization(string specialization)
    {
        var doctors = await _doctorService.GetBySpecializationAsync(specialization);
        return Ok(ApiResponse<IEnumerable<DoctorResponseDto>>.SuccessResponse(doctors));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DoctorResponseDto>>> Create([FromBody] DoctorCreateDto doctorDto)
    {
        var doctor = await _doctorService.CreateAsync(doctorDto);
        if (doctor == null)
        {
            return BadRequest(ApiResponse<DoctorResponseDto>.ErrorResponse("Failed to create doctor. Email or license number may already exist."));
        }
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, ApiResponse<DoctorResponseDto>.SuccessResponse(doctor, "Doctor created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult<ApiResponse<DoctorResponseDto>>> Update(int id, [FromBody] DoctorUpdateDto doctorDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Doctor")
        {
            var currentDoctor = await _doctorService.GetByUserIdAsync(userId);
            if (currentDoctor == null || currentDoctor.Id != id)
            {
                return Forbid();
            }
        }
        var doctor = await _doctorService.UpdateAsync(id, doctorDto);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorResponseDto>.ErrorResponse(Constants.Messages.DoctorNotFound));
        }
        return Ok(ApiResponse<DoctorResponseDto>.SuccessResponse(doctor, "Doctor updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _doctorService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(Constants.Messages.DoctorNotFound));
        }
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Doctor deleted successfully"));
    }
}