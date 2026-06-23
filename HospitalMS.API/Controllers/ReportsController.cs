using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Reports;
using HospitalMS.BL.Services;
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
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<ReportsController> _logger;
    public ReportsController(IReportingService reportingService, IAppointmentService appointmentService, IDoctorService doctorService, IPatientService patientService, ILogger<ReportsController> logger)
    {
        _reportingService = reportingService;
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
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
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
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
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
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
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
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
            var today = await _appointmentService.GetAppointmentsCountAsync(doctor.Id, null, DateTime.UtcNow.Date);
            stats = new DashboardStatsDto { TotalAppointments = performance.TotalAppointments, CompletedAppointments = performance.CompletedAppointments, AppointmentsToday = today, ApprovalRate = performance.ApprovalRate, PatientsServed = performance.PatientsServed };
        }
        else if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null)
            {
                return NotFound(ApiResponse<DashboardStatsDto>.ErrorResponse("Patient profile not found"));
            }
            var total = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id);
            var upcoming = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, statuses: new[] { HospitalMS.Models.Enums.AppointmentStatus.Scheduled }, fromDate: DateTime.UtcNow);
            var completed = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, statuses: new[] { HospitalMS.Models.Enums.AppointmentStatus.Completed });
            stats = new DashboardStatsDto { TotalAppointments = total, UpcomingAppointments = upcoming, CompletedAppointments = completed };
        }
        else
        {
            return Forbid();
        }

        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
    }

    [HttpGet("dashboard/admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminDashboardApiDto>>> GetAdminDashboard()
    {
        var systemStats = await _reportingService.GetSystemStatisticsAsync();
        var recentAppointments = await _appointmentService.GetRecentAppointmentsAsync(5);
        var dto = new AdminDashboardApiDto
        {
            TotalDoctors = systemStats.TotalDoctors,
            TotalPatients = systemStats.TotalPatients,
            TotalAppointments = systemStats.TotalAppointments,
            AppointmentsToday = systemStats.AppointmentsToday,
            PendingApprovals = systemStats.PendingApprovals,
            CompletionRate = systemStats.CompletionRate,
            NoShowRate = systemStats.NoShowRate,
            RecentAppointments = recentAppointments.Select(a => new RecentAppointmentSummaryDto
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Status = a.Status,
                AppointmentDate = a.AppointmentDate
            }).ToList()
        };

        _logger.LogInformation("Admin dashboard data retrieved");
        return Ok(ApiResponse<AdminDashboardApiDto>.SuccessResponse(dto));
    }

    [HttpGet("dashboard/doctor")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DoctorDashboardApiDto>>> GetDoctorDashboard()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        var doctor = await _doctorService.GetByUserIdAsync(userId);

        if (doctor == null)
            return NotFound(ApiResponse<DoctorDashboardApiDto>.ErrorResponse("Doctor profile not found"));

        var performance = await _reportingService.GenerateDoctorPerformanceReportAsync(doctor.Id);
        var todayCount = await _appointmentService.GetAppointmentsCountAsync(doctor.Id, null, DateTime.UtcNow.Date);
        var pendingApprovals = await _appointmentService.GetPendingApprovalsAsync(doctor.Id);
        var upcoming = await _appointmentService.GetUpcomingByDoctorIdAsync(doctor.Id, 5);
        var dto = new DoctorDashboardApiDto
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            Specialization = doctor.Specialization,
            IsAvailable = doctor.IsAvailable,
            TodayAppointmentsCount = todayCount,
            PendingApprovalsCount = pendingApprovals.Count(),
            TotalAppointments = performance.TotalAppointments,
            CompletedAppointments = performance.CompletedAppointments,
            ApprovalRate = performance.ApprovalRate,
            PatientsServed = performance.PatientsServed,
            UpcomingAppointments = upcoming.Select(a => new RecentAppointmentSummaryDto
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Status = a.Status,
                AppointmentDate = a.AppointmentDate
            }).ToList()
        };

        _logger.LogInformation("Doctor dashboard data retrieved for doctor {DoctorId}", doctor.Id);
        return Ok(ApiResponse<DoctorDashboardApiDto>.SuccessResponse(dto));
    }

    [HttpGet("dashboard/patient")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PatientDashboardApiDto>>> GetPatientDashboard()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        var patient = await _appointmentService.GetPatientByUserIdAsync(userId);

        if (patient == null)
            return NotFound(ApiResponse<PatientDashboardApiDto>.ErrorResponse("Patient profile not found"));

        var total = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id);
        var upcoming = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, isUpcomingDashboard: true);
        var completed = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, statuses: new[] { HospitalMS.Models.Enums.AppointmentStatus.Completed });
        var cancelled = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, isCancelledOrRejected: true);
        var pending = await _appointmentService.GetPatientAppointmentsCountAsync(patient.Id, statuses: new[] { HospitalMS.Models.Enums.AppointmentStatus.Scheduled }, approvalStatuses: new[] { HospitalMS.Models.Enums.AppointmentApprovalStatus.Pending });
        var recent = await _appointmentService.GetRecentAppointmentsForPatientAsync(patient.Id, 5);

        var dto = new PatientDashboardApiDto
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            UpcomingAppointmentsCount = upcoming,
            CompletedAppointmentsCount = completed,
            CancelledAppointmentsCount = cancelled,
            PendingApprovalsCount = pending,
            TotalAppointments = total,
            RecentAppointments = recent.Select(a => new RecentAppointmentSummaryDto
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Status = a.Status,
                AppointmentDate = a.AppointmentDate
            }).ToList()
        };

        _logger.LogInformation("Patient dashboard data retrieved for patient {PatientId}", patient.Id);
        return Ok(ApiResponse<PatientDashboardApiDto>.SuccessResponse(dto));
    }

    [HttpGet("cards")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCardDetails([FromQuery] string type)
    {
        switch (type)
        {
            case "Doctors":
                var doctors = await _doctorService.GetAllAsync();
                return Ok(ApiResponse<object>.SuccessResponse(doctors.Select(d => new
                {
                    d.Id,
                    name = d.FullName,
                    d.Email,
                    d.Specialization,
                    experience = d.YearsOfExperience,
                    d.IsAvailable
                })));

            case "Patients":
                var patients = await _patientService.GetAllAsync();
                return Ok(ApiResponse<object>.SuccessResponse(patients.Select(p => new
                {
                    p.Id,
                    name = p.FullName,
                    p.Email,
                    phone = p.PhoneNumber,
                    p.BloodGroup
                })));

            case "TodayAppointments":
                var todayAppts = await _appointmentService.GetTodaysAppointmentsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(todayAppts.Select(a => new
                {
                    patient = a.PatientName,
                    doctor = a.DoctorName,
                    date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                    time = a.StartTime.ToString(@"hh\:mm"),
                    a.Status
                })));

            case "PendingApprovals":
                var pending = await _appointmentService.GetAllPendingApprovalsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(pending.Select(a => new
                {
                    patient = a.PatientName,
                    doctor = a.DoctorName,
                    date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                    a.Status
                })));

            default:
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid type. Use: Doctors, Patients, TodayAppointments, PendingApprovals"));
        }
    }

    [HttpGet("full")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FullReportApiDto>>> GetFullReport()
    {
        var report = await _reportingService.GenerateAppointmentReportAsync(null, null);
        var doctors = await _doctorService.GetAllAsync();
        var patients = await _patientService.GetAllAsync();
        var todayAppts = await _appointmentService.GetTodaysAppointmentsAsync();
        var dto = new FullReportApiDto
        {
            Stats = report,
            Doctors = doctors.Select(d => new DoctorSummaryDto
            {
                Id = d.Id,
                Name = d.FullName,
                Email = d.Email,
                Specialization = d.Specialization,
                YearsOfExperience = d.YearsOfExperience,
                Phone = d.PhoneNumber
            }).ToList(),

            Patients = patients.Select(p => new PatientSummaryDto
            {
                Id = p.Id,
                Name = p.FullName,
                Email = p.Email,
                Phone = p.PhoneNumber,
                BloodGroup = p.BloodGroup,
                DateOfBirth = p.DateOfBirth
            }).ToList(),

            TodayAppointments = todayAppts.Select(a => new RecentAppointmentSummaryDto
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Status = a.Status,
                AppointmentDate = a.AppointmentDate
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Full report snapshot generated");
        return Ok(ApiResponse<FullReportApiDto>.SuccessResponse(dto));
    }
}
