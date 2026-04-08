using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly IReportingService _reportingService;
    public AdminController(IDoctorService doctorService, IPatientService patientService, IAppointmentService appointmentService, IReportingService reportingService)
    {
        _doctorService = doctorService;
        _patientService = patientService;
        _appointmentService = appointmentService;
        _reportingService = reportingService;
    }

    // admin dashboard
    public async Task<IActionResult> Dashboard()
    {
        var recentAppointments = await _appointmentService.GetRecentAppointmentsAsync(5);
        var systemStats = await _reportingService.GetSystemStatisticsAsync();
        var model = new AdminDashboardViewModel
        {
            TotalDoctors = systemStats.TotalDoctors,
            TotalPatients = systemStats.TotalPatients,
            TotalAppointments = systemStats.TotalAppointments,
            AppointmentsToday = systemStats.AppointmentsToday,
            PendingApprovals = systemStats.PendingApprovals,
            CompletionRate = systemStats.CompletionRate,
            NoShowRate = systemStats.NoShowRate,
            RecentAppointments = recentAppointments.Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Status = a.Status,
                StatusEnum = a.StatusEnum,
                AppointmentDate = a.AppointmentDate
            }).ToList()
        };
       
        return View(model);
    }

    // get card details for dashboard (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetCardDetails(string type)
    {
        switch (type)
        {
            case "Doctors":
                var doctors = await _doctorService.GetAllAsync();
                return Json(doctors.Select(d => new
                {
                    name = d.FullName,
                    email = d.Email,
                    specialization = d.Specialization,
                    experience = d.YearsOfExperience
                }));
            
            case "Patients":
                var patients = await _patientService.GetAllAsync();
                return Json(patients.Select(p => new
                {
                    name = p.FullName,
                    email = p.Email,
                    phone = p.PhoneNumber,
                    bloodGroup = p.BloodGroup
                }));
            
            case "TodayAppointments":
                var all = await _appointmentService.GetAllAsync();
                var todayUtc = DateTime.UtcNow.Date;
                var todays = all.Where(a => a.AppointmentDate.Date == todayUtc);
                return Json(todays.Select(a => new
                {
                    patient = a.PatientName,
                    doctor = a.DoctorName,
                    date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                    time = a.StartTime.ToString(@"hh\:mm"),
                    status = a.Status
                }));
           
            case "PendingApprovals":
                var appointments = await _appointmentService.GetAllAsync();
                var pending = appointments
                    .Where(a =>
                        a.ApprovalStatusEnum ==
                            HospitalMS.Models.Enums.AppointmentApprovalStatus.Pending
                        &&
                        a.StatusEnum ==
                            HospitalMS.Models.Enums.AppointmentStatus.Scheduled
                    );
                return Json(pending.Select(a => new
                {
                    patient = a.PatientName,
                    doctor = a.DoctorName,
                    date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                    status = a.Status
                }));
        }

        return Json(new { });
    }

    // generate appointment report
    public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate)
    {
        var report = await _reportingService.GenerateAppointmentReportAsync(startDate, endDate);
        
        return Json(report);
    }

    // get full report data (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetFullReportData()
    {
        var report = await _reportingService.GenerateAppointmentReportAsync(null, null);
        var doctors = await _doctorService.GetAllAsync();
        var doctorList = doctors.Select(d => new
        {
            name = d.FullName,
            email = d.Email,
            specialization = d.Specialization,
            experience = d.YearsOfExperience,
            phone = d.PhoneNumber
        }).ToList();
        var patients = await _patientService.GetAllAsync();
        var patientList = patients.Select(p => new
        {
            name = p.FullName,
            email = p.Email,
            phone = p.PhoneNumber,
            bloodGroup = p.BloodGroup,
            dateOfBirth = p.DateOfBirth.ToString("MMM dd, yyyy")
        }).ToList();
        var allAppointments = await _appointmentService.GetAllAsync();
        var todayUtc = DateTime.UtcNow.Date;
        var todayAppointments = allAppointments
            .Where(a => a.AppointmentDate.Date == todayUtc)
            .Select(a => new
            {
                patient = a.PatientName,
                doctor = a.DoctorName,
                date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                time = a.StartTime.ToString(@"hh\:mm"),
                status = a.Status
            }).ToList();
       
        return Json(new
        {
            stats = report,
            doctors = doctorList,
            patients = patientList,
            todayAppointments = todayAppointments,
            generatedAt = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt")
        });
    }

    // get monthly trend data
    public async Task<IActionResult> GetMonthlyTrend(int months = 12)
    {
        var trend = await _reportingService.GetMonthlyTrendAsync(months);
        
        return Json(trend.Months);
    }
}