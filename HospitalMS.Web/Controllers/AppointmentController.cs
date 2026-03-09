using HospitalMS.BL.DTOs.Appointment;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers;

[Authorize]
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentWorkflowService _workflowService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ISearchService _searchService;
    private readonly IExportService _exportService;
    private readonly IBillingService _billingService;
    private readonly ILogger<AppointmentController> _logger;
    public AppointmentController(IAppointmentService appointmentService, IAppointmentWorkflowService workflowService, IDoctorService doctorService, IPatientService patientService, ISearchService searchService, IExportService exportService, IBillingService billingService, ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _workflowService = workflowService;
        _doctorService = doctorService;
        _patientService = patientService;
        _searchService = searchService;
        _exportService = exportService;
        _billingService = billingService;
        _logger = logger;
    }

    // list appointments
    public async Task<IActionResult> Index(string? searchQuery, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        int? doctorId = null;
        int? patientId = null;
        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            doctorId = doctor?.Id;
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            patientId = patient?.Id;
        }
        var searchResult = await _searchService.SearchAppointmentsAsync(searchQuery ?? string.Empty, doctorId, patientId, fromDate, toDate, null, page, pageSize);
        var model = new AppointmentListViewModel
        {
            Appointments = searchResult.Items.Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                PatientName = a.PatientName,
                DoctorName = a.DoctorName,
                Specialization = a.Specialization,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason
            }).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(searchResult.TotalCount / (double)pageSize),
            PageSize = pageSize,
            TotalCount = searchResult.TotalCount,
            SearchQuery = searchQuery,
            FromDate = fromDate,
            ToDate = toDate
        };
        return View(model);
    }

    // pending doctor approvals
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> PendingApprovals()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null)
            return NotFound("Doctor profile not found");
        var pendingAppointments = await _workflowService.GetPendingApprovalsAsync(doctor.Id);
        var model = pendingAppointments.Select(a => new AppointmentViewModel
        {
            Id = a.Id,
            PatientName = a.PatientName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Reason = a.Reason,
            Status = a.Status
        }).OrderBy(a => a.AppointmentDate).ToList();
        return View(model);
    }

    // get available slots (AJAX)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
    {
        try
        {
            var slots = await _workflowService.GetAvailableSlotsAsync(doctorId, date);
            if (slots == null || !slots.Any())
            {
                return Json(new { success = true, slots = new List<object>(), message = "No slots available for this date" });
            }
            var formattedSlots = slots.Select(slot => new
            {
                start = slot.StartTime.ToString(@"hh\:mm\:ss"),
                end = slot.EndTime.ToString(@"hh\:mm\:ss"),
                display = FormatTimeDisplay(slot.StartTime),
                available = slot.IsAvailable
            }).ToList();
            return Json(new { success = true, slots = formattedSlots });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting available slots for doctor {doctorId}");
            return Json(new { success = false, message = ex.Message, slots = new List<object>() });
        }
    }

    // get appointments by status popup (AJAX)
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetByStatusPopup(string status)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var patient = await _patientService.GetByUserIdAsync(userId);
        if (patient == null)
            return Json(new List<object>());
        var appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
        var filtered = appointments.Where(a =>
            status switch
            {
                "Upcoming" =>
                    (a.Status == "Scheduled" || a.Status == "Confirmed")
                    && a.ApprovalStatus == "Approved"
                    && a.AppointmentDate.Date >= DateTime.Today,
                "Pending" =>
                    a.ApprovalStatus == "Pending"
                    && a.Status == "Scheduled",
                "Completed" =>
                    a.Status == "Completed",
                "Cancelled" =>
                    a.Status == "Cancelled"
                    || a.Status == "NoShow"
                    || a.ApprovalStatus == "Rejected",
                _ => false
            }).OrderByDescending(a => a.AppointmentDate)
              .ToList();
        return Json(filtered.Select(a => new
        {
            id = a.Id,
            doctorName = a.DoctorName,
            date = a.AppointmentDate.ToString("MMM dd, yyyy"),
            time = DateTime.Today.Add(a.StartTime).ToString("hh:mm tt"),
            status = a.Status
        }));
    }

    // format time as AM/PM
    private string FormatTimeDisplay(TimeSpan time)
    {
        var hours = time.Hours;
        var minutes = time.Minutes;
        var ampm = hours >= 12 ? "PM" : "AM";
        var displayHours = hours % 12;
        if (displayHours == 0) displayHours = 12;
        return $"{displayHours:D2}:{minutes:D2} {ampm}";
    }

    // show appointment request form
    [HttpGet]
    [ActionName("Request")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> RequestAppointment(int doctorId)
    {
        var doctor = await _doctorService.GetByIdAsync(doctorId);
        if (doctor == null)
            return NotFound();
        var model = new AppointmentRequestViewModel
        {
            DoctorId = doctorId,
            DoctorName = doctor.FullName,
            Specialization = doctor.Specialization,
            ConsultationFee = doctor.ConsultationFee,
            AppointmentDate = DateTime.Today.AddDays(1)
        };
        return View(model);
    }

    // submit appointment request
    [HttpPost]
    [ActionName("Request")]
    [Authorize(Roles = "Patient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestAppointment(AppointmentRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ReloadDoctorData(model);
            return View(model);
        }
        if (model.EndTime <= model.StartTime)
        {
            ModelState.AddModelError("", "End time must be greater than start time.");
            await ReloadDoctorData(model);
            return View(model);
        }
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            var userId = int.Parse(userIdClaim);
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null)
                return Unauthorized();
            var dto = new AppointmentCreateDto
            {
                PatientId = patient.Id,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Reason = model.Reason
            };
            await _appointmentService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Appointment requested successfully! Awaiting doctor approval.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await ReloadDoctorData(model);
            return View(model);
        }
    }

    // reload doctor info helper
    private async Task ReloadDoctorData(AppointmentRequestViewModel model)
    {
        var doctor = await _doctorService.GetByIdAsync(model.DoctorId);
        if (doctor != null)
        {
            model.DoctorName = doctor.FullName;
            model.Specialization = doctor.Specialization;
            model.ConsultationFee = doctor.ConsultationFee;
        }
    }


    // approve appointment
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Approve(int appointmentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Doctor profile not found." });
                }
                TempData["ErrorMessage"] = "Doctor profile not found.";
                return RedirectToAction(nameof(PendingApprovals));
            }
            var result = await _workflowService.ApproveAppointmentAsync(appointmentId, doctor.Id);
            if (result == null)
            {
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to approve appointment." });
                }
                TempData["ErrorMessage"] = "Failed to approve appointment.";
                return RedirectToAction(nameof(PendingApprovals));
            }
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Appointment approved successfully!" });
            }
            TempData["SuccessMessage"] = "Appointment approved successfully!";
            return RedirectToAction(nameof(PendingApprovals));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving appointment");
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "An error occurred." });
            }
            TempData["ErrorMessage"] = "An error occurred.";
            return RedirectToAction(nameof(PendingApprovals));
        }
    }

    // reject appointment
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Reject(int appointmentId, string rejectionReason)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Doctor profile not found." });
                }
                TempData["ErrorMessage"] = "Doctor profile not found.";
                return RedirectToAction(nameof(PendingApprovals));
            }
            var result = await _workflowService.RejectAppointmentAsync(appointmentId, doctor.Id, rejectionReason);
            if (result == null)
            {
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to reject appointment." });
                }
                TempData["ErrorMessage"] = "Failed to reject appointment.";
                return RedirectToAction(nameof(PendingApprovals));
            }
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Appointment rejected." });
            }
            TempData["SuccessMessage"] = "Appointment rejected.";
            return RedirectToAction(nameof(PendingApprovals));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting appointment");
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "An error occurred." });
            }
            TempData["ErrorMessage"] = "An error occurred.";
            return RedirectToAction(nameof(PendingApprovals));
        }
    }

    // complete appointment
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Complete(int appointmentId, string? diagnosis, string? prescription, string? notes)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Doctor profile not found.";
            return RedirectToAction(nameof(Index));
        }
        var result = await _workflowService.CompleteAppointmentAsync(appointmentId, doctor.Id, diagnosis, prescription, notes);
        if (result == null)
        {
            TempData["ErrorMessage"] = "Failed to complete appointment.";
            return RedirectToAction(nameof(Index));
        }
        TempData["SuccessMessage"] = "Appointment completed successfully!";
        return RedirectToAction(nameof(Details), new { id = appointmentId });
    }

    // cancel appointment
    [HttpPost]
    public async Task<IActionResult> Cancel(int appointmentId, string? reason)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var result = await _workflowService.CancelAppointmentAsync(appointmentId, userId, userEmail, reason);
        if (!result)
        {
            TempData["ErrorMessage"] = "Failed to cancel appointment.";
            return RedirectToAction(nameof(Index));
        }
        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }

    // show reschedule form
    [HttpGet]
    public async Task<IActionResult> Reschedule(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();
        // only allow rescheduling of Scheduled or Confirmed appointments
        if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = "Only scheduled or confirmed appointments can be rescheduled.";
            return RedirectToAction(nameof(Details), new { id });
        }
        var model = new RescheduleAppointmentViewModel
        {
            AppointmentId = appointment.Id,
            DoctorName = appointment.DoctorName,
            DoctorId = appointment.DoctorId,
            CurrentDate = appointment.AppointmentDate,
            CurrentStartTime = appointment.StartTime,
            NewDate = appointment.AppointmentDate.AddDays(1),
            NewStartTime = appointment.StartTime,
            NewEndTime = appointment.EndTime
        };
        return View(model);
    }

    // reschedule appointment (submit)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(RescheduleAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (model.NewEndTime <= model.NewStartTime)
        {
            ModelState.AddModelError("", "End time must be after start time.");
            return View(model);
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _workflowService.RescheduleAppointmentAsync(model.AppointmentId, userId, model.NewDate, model.NewStartTime, model.NewEndTime);
        if (result == null)
        {
            ModelState.AddModelError("", "Failed to reschedule. The selected time slot may be unavailable.");
            return View(model);
        }
        TempData["SuccessMessage"] = "Appointment rescheduled successfully!";
        return RedirectToAction(nameof(Details), new { id = model.AppointmentId });
    }

    // appointment details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();
        var invoice = await _billingService.GetInvoiceByAppointmentIdAsync(id);
        var model = new AppointmentDetailViewModel
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            DoctorName = appointment.DoctorName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            HasInvoice = invoice != null,
            InvoiceId = invoice?.Id,
            IsInvoicePaid = invoice?.IsPaid ?? false,
            Diagnosis = appointment.Diagnosis,
            Prescription = appointment.Prescription,
            Notes = appointment.Notes
        };
        return View(model);
    }

    // mark as no-show
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> MarkNoShow(int appointmentId)
    {
        var result = await _workflowService.MarkAsNoShowAsync(appointmentId);
        if (!result)
        {
            TempData["ErrorMessage"] = "Failed to mark appointment as no-show.";
            return RedirectToAction(nameof(Index));
        }
        TempData["SuccessMessage"] = "Appointment marked as no-show.";
        return RedirectToAction(nameof(Index));
    }

    // view appointment history
    [HttpGet]
    public async Task<IActionResult> History(string? status, DateTime? fromDate, DateTime? toDate, string? doctorName)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<AppointmentResponseDto> appointments = new List<AppointmentResponseDto>();
        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor != null)
            {
                appointments = await _appointmentService.GetByDoctorIdAsync(doctor.Id);
            }
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            if (patient != null)
            {
                appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
            }
        }
        else
        {
            appointments = await _appointmentService.GetAllAsync();
        }
        var query = appointments.AsQueryable();
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }
        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= toDate.Value.AddDays(1));
        }
        if (!string.IsNullOrEmpty(doctorName))
        {
            query = query.Where(a => a.DoctorName.Contains(doctorName, StringComparison.OrdinalIgnoreCase));
        }
        var model = query.Select(a => new AppointmentViewModel
        {
            Id = a.Id,
            PatientName = a.PatientName,
            DoctorName = a.DoctorName,
            Specialization = a.DoctorSpecialization,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason
        }).OrderByDescending(a => a.AppointmentDate).ToList();
        return View(model);
    }

    // appointment status tracking view
    [HttpGet]
    public async Task<IActionResult> StatusTracking(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();
        var model = new AppointmentDetailViewModel
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            DoctorName = appointment.DoctorName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            Diagnosis = appointment.Diagnosis,
            Prescription = appointment.Prescription,
            Notes = appointment.Notes
        };
        return View(model);
    }

    // view audit trail
    [HttpGet]
    public async Task<IActionResult> AuditTrail(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();
        var model = new AppointmentDetailViewModel
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            DoctorName = appointment.DoctorName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Reason = appointment.Reason,
            Diagnosis = appointment.Diagnosis,
            Prescription = appointment.Prescription,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };
        return View(model);
    }

    // export to CSV
    [HttpGet]
    public async Task<IActionResult> ExportToCSV()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointments = await GetUserAppointmentsAsync(userId);
        var exportData = appointments.Select(a => new AppointmentExportDto
        {
            Id = a.Id,
            PatientName = a.PatientName,
            PatientEmail = a.PatientEmail ?? "",
            DoctorName = a.DoctorName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason ?? string.Empty,
            Diagnosis = a.Diagnosis,
            Prescription = a.Prescription
        }).ToList();
        var csvBytes = await _exportService.ExportAppointmentsToCSVAsync(exportData);
        return File(csvBytes, "text/csv", $"appointments_{DateTime.Now:yyyyMMdd}.csv");
    }

    // export to excel
    [HttpGet]
    public async Task<IActionResult> ExportToExcel()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointments = await GetUserAppointmentsAsync(userId);
        var exportData = appointments.Select(a => new AppointmentExportDto
        {
            Id = a.Id,
            PatientName = a.PatientName,
            PatientEmail = a.PatientEmail ?? "",
            DoctorName = a.DoctorName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason ?? string.Empty,
            Diagnosis = a.Diagnosis,
            Prescription = a.Prescription
        }).ToList();
        var excelBytes = await _exportService.ExportAppointmentsToExcelAsync(exportData);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"appointments_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // export to pdf
    [HttpGet]
    public async Task<IActionResult> ExportToPDF()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var appointments = await GetUserAppointmentsAsync(userId);
        var exportData = appointments.Select(a => new AppointmentExportDto
        {
            Id = a.Id,
            PatientName = a.PatientName,
            PatientEmail = a.PatientEmail ?? "",
            DoctorName = a.DoctorName,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason ?? string.Empty,
            Diagnosis = a.Diagnosis,
            Prescription = a.Prescription
        }).ToList();
        var pdfBytes = await _exportService.ExportAppointmentsToPdfAsync(exportData);
        return File(pdfBytes, "application/pdf", $"appointments_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // get user appointments helper
    private async Task<IEnumerable<AppointmentResponseDto>> GetUserAppointmentsAsync(int userId)
    {
        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            return doctor != null ? await _appointmentService.GetByDoctorIdAsync(doctor.Id) : new List<AppointmentResponseDto>();
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            return patient != null ? await _appointmentService.GetByPatientIdAsync(patient.Id) : new List<AppointmentResponseDto>();
        }
        else
        {
            return await _appointmentService.GetAllAsync();
        }
    }
}