using HospitalMS.BL.Common;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<ReportsController> _logger;
    public ReportsController(IReportingService reportingService, IAppointmentService appointmentService, ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet("appointments")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AppointmentReportDto>>> GetAppointmentReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var report = await _reportingService.GenerateAppointmentReportAsync(startDate, endDate);
        _logger.LogInformation("Appointment report generated for period {StartDate} to {EndDate}", startDate?.ToString("yyyy-MM-dd") ?? "all time", endDate?.ToString("yyyy-MM-dd") ?? "present");
        return Ok(ApiResponse<AppointmentReportDto>.SuccessResponse(report));
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DoctorPerformanceReportDto>>> GetDoctorPerformanceReport(int doctorId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Doctor")
        {
            var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
            if (doctor == null || doctor.Id != doctorId)
            {
                return Forbid();
            }
        }
        var report = await _reportingService.GenerateDoctorPerformanceReportAsync(doctorId);
        _logger.LogInformation("Doctor performance report generated for doctor {DoctorId}", doctorId);
        return Ok(ApiResponse<DoctorPerformanceReportDto>.SuccessResponse(report));
    }

    [HttpGet("my-performance")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DoctorPerformanceReportDto>>> GetMyPerformanceReport()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorPerformanceReportDto>.ErrorResponse("Doctor profile not found"));
        }
        var report = await _reportingService.GenerateDoctorPerformanceReportAsync(doctor.Id);
        return Ok(ApiResponse<DoctorPerformanceReportDto>.SuccessResponse(report));
    }

    [HttpGet("system-stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SystemStatisticsDto>>> GetSystemStatistics()
    {
        var stats = await _reportingService.GetSystemStatisticsAsync();
        _logger.LogInformation("System statistics retrieved");
        return Ok(ApiResponse<SystemStatisticsDto>.SuccessResponse(stats));
    }

    [HttpGet("monthly-trend")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MonthlyTrendDto>>> GetMonthlyTrend([FromQuery] int months = 12)
    {
        if (months < 1 || months > 24)
        {
            return BadRequest(ApiResponse<MonthlyTrendDto>.ErrorResponse("Months must be between 1 and 24"));
        }
        var trend = await _reportingService.GetMonthlyTrendAsync(months);
        _logger.LogInformation("Monthly trend data retrieved for {Months} months", months);
        return Ok(ApiResponse<MonthlyTrendDto>.SuccessResponse(trend));
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        DashboardStatsDto stats;
        if (role == "Admin")
        {
            var systemStats = await _reportingService.GetSystemStatisticsAsync();
            stats = new DashboardStatsDto { TotalDoctors = systemStats.TotalDoctors, TotalPatients = systemStats.TotalPatients, TotalAppointments = systemStats.TotalAppointments, AppointmentsToday = systemStats.AppointmentsToday, PendingApprovals = systemStats.PendingApprovals, CompletionRate = systemStats.CompletionRate, NoShowRate = systemStats.NoShowRate };
        }
        else if (role == "Doctor")
        {
            var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
            if (doctor == null)
            {
                return NotFound(ApiResponse<DashboardStatsDto>.ErrorResponse("Doctor profile not found"));
            }
            var performance = await _reportingService.GenerateDoctorPerformanceReportAsync(doctor.Id);
            var todayAppointments = await _appointmentService.GetByDoctorIdAsync(doctor.Id);
            var today = todayAppointments.Count(a => a.AppointmentDate.Date == DateTime.UtcNow.Date);
            stats = new DashboardStatsDto { TotalAppointments = performance.TotalAppointments, CompletedAppointments = performance.CompletedAppointments, AppointmentsToday = today, ApprovalRate = performance.ApprovalRate, PatientsServed = performance.PatientsServed };
        }
        else if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null)
            {
                return NotFound(ApiResponse<DashboardStatsDto>.ErrorResponse("Patient profile not found"));
            }
            var appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
            var upcoming = appointments.Count(a => a.AppointmentDate >= DateTime.UtcNow && a.Status == "Scheduled");
            var completed = appointments.Count(a => a.Status == "Completed");
            stats = new DashboardStatsDto { TotalAppointments = appointments.Count(), UpcomingAppointments = upcoming, CompletedAppointments = completed };
        }
        else
        {
            return Forbid();
        }
        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
    }
}

public class DashboardStatsDto
{
    public int TotalDoctors { get; set; }

    public int TotalPatients { get; set; }

    public int TotalAppointments { get; set; }

    public int AppointmentsToday { get; set; }

    public int UpcomingAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int PendingApprovals { get; set; }

    public decimal CompletionRate { get; set; }

    public decimal NoShowRate { get; set; }

    public decimal ApprovalRate { get; set; }

    public int PatientsServed { get; set; }
}