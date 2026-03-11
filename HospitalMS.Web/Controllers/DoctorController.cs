using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers;

[Authorize]
public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentWorkflowService _workflowService;
    private readonly ISearchService _searchService;
    private readonly IWorkingHoursService _workingHoursService;
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DoctorController> _logger;
    public DoctorController(IDoctorService doctorService, IAppointmentService appointmentService, IAppointmentWorkflowService workflowService, ISearchService searchService, IWorkingHoursService workingHoursService, IDepartmentService departmentService, ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _workflowService = workflowService;
        _searchService = searchService;
        _workingHoursService = workingHoursService;
        _departmentService = departmentService;
        _logger = logger;
    }

    // doctor dashboard
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null) return NotFound("Doctor profile not found");
            var today = DateTime.Today;
            var todayAppointmentsCount = await _appointmentService.GetAppointmentsCountAsync(doctor.Id, null, today);
            var pendingApprovals = await _workflowService.GetPendingApprovalsAsync(doctor.Id);
            var completedCount = await _appointmentService.GetAppointmentsCountAsync(doctor.Id, HospitalMS.Models.Enums.AppointmentStatus.Completed);
            var upcomingAppointments = await _appointmentService.GetUpcomingByDoctorIdAsync(doctor.Id, 5);

            var model = new DoctorDashboardViewModel
            {
                Doctor = new DoctorViewModel
                {
                    Id = doctor.Id,
                    FullName = doctor.FullName,
                    Specialization = doctor.Specialization,
                    IsAvailable = doctor.IsAvailable
                },
                TodayAppointmentsCount = todayAppointmentsCount,
                PendingApprovalsCount = pendingApprovals.Count(),
                TotalPatientsTreated = completedCount,
                UpcomingAppointments = upcomingAppointments.Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    PatientName = a.PatientName,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    Status = a.Status
                }).ToList()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctor dashboard");
            return View("Error");
        }
    }

    // list doctors
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchQuery, string? specialization, int page = 1, int pageSize = 9)
    {
        try
        {
            IEnumerable<BL.DTOs.Doctor.DoctorResponseDto> doctors;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchResults = await _searchService.SearchDoctorsAsync(searchQuery);
                doctors = searchResults.Select(s => new BL.DTOs.Doctor.DoctorResponseDto
                {
                    Id = s.Id,
                    FullName = s.Name,
                    Email = "",
                    Specialization = s.Specialization,
                    LicenseNumber = s.LicenseNumber,
                    YearsOfExperience = 0,
                    ConsultationFee = 0,
                    IsAvailable = true,
                    Bio = "",
                    Qualifications = ""
                });
            }
            else
            {
                doctors = await _doctorService.GetAllAsync();
            }
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                doctors = doctors.Where(d => d.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase));
            }
            var allDoctors = await _doctorService.GetAllAsync();
            var specializations = allDoctors.Select(d => d.Specialization).Distinct().OrderBy(s => s).ToList();
            var pagedResult = PaginationService.Create(doctors, page, pageSize);
            var model = new DoctorListViewModel
            {
                Doctors = pagedResult.Items.Select(d => new DoctorViewModel
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Email = d.Email,
                    Specialization = d.Specialization,
                    YearsOfExperience = d.YearsOfExperience,
                    IsAvailable = d.IsAvailable,
                    ConsultationFee = d.ConsultationFee,
                    Qualifications = d.Qualifications,
                    PhoneNumber = d.PhoneNumber
                }).ToList(),
                CurrentPage = pagedResult.PageNumber,
                TotalPages = pagedResult.TotalPages,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                SearchQuery = searchQuery,
                SelectedSpecialization = specialization,
                Specializations = specializations
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctors");
            return View("Error");
        }
    }

    // doctor details
    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();
        var model = new DoctorViewModel
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Email = doctor.Email,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            YearsOfExperience = doctor.YearsOfExperience,
            ConsultationFee = doctor.ConsultationFee,
            IsAvailable = doctor.IsAvailable,
            Bio = doctor.Bio,
            Qualifications = doctor.Qualifications,
            PhoneNumber = doctor.PhoneNumber,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.DepartmentName,
            CreatedBy = doctor.CreatedBy,
            UpdatedBy = doctor.UpdatedBy
        };
        return View(model);
    }

    // doctor appointment calendar
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Calendar(int month = 0, int year = 0)
    {
        try
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null) return NotFound("Doctor profile not found");
            var appointments = await _appointmentService.GetByDoctorIdAsync(doctor.Id);
            var appointmentList = appointments.ToList();
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var daysInMonth = lastDayOfMonth.Day;
            var startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            var days = new List<CalendarDayViewModel>();
            var prevMonthLastDay = firstDayOfMonth.AddDays(-1);
            for (int i = startingDayOfWeek - 1; i >= 0; i--)
            {
                var date = prevMonthLastDay.AddDays(-i);
                days.Add(new CalendarDayViewModel { Day = date.Day, IsCurrentMonth = false, Date = date });
            }
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayAppointments = appointmentList
                    .Where(a => a.AppointmentDate.Date == date.Date)
                    .Select(a => new UpcomingAppointmentViewModel
                    {
                        Id = a.Id,
                        PatientName = a.PatientName,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        Reason = a.Reason ?? string.Empty
                    }).ToList();
                days.Add(new CalendarDayViewModel
                {
                    Day = day,
                    IsCurrentMonth = true,
                    Date = date,
                    AppointmentCount = dayAppointments.Count,
                    Appointments = dayAppointments
                });
            }
            int remainingDays = 42 - days.Count;
            for (int day = 1; day <= remainingDays; day++)
            {
                var date = lastDayOfMonth.AddDays(day);
                days.Add(new CalendarDayViewModel { Day = date.Day, IsCurrentMonth = false, Date = date });
            }
            var model = new CalendarViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.FullName,
                CurrentMonth = month,
                CurrentYear = year,
                Days = days,
                UpcomingAppointments = appointmentList
                    .Where(a => a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(10)
                    .Select(a => new UpcomingAppointmentViewModel
                    {
                        Id = a.Id,
                        PatientName = a.PatientName,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        Reason = a.Reason ?? string.Empty
                    }).ToList(),
                TotalAppointments = appointmentList.Count
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading calendar");
            return View("Error");
        }
    }

    // show create form
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _departmentService.GetAllAsync(), "Id", "Name");
        return View();
    }

    // save new doctor
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var createDto = new BL.DTOs.Doctor.DoctorCreateDto
            {
                Email = model.Email,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Specialization = model.Specialization,
                LicenseNumber = model.LicenseNumber,
                YearsOfExperience = model.YearsOfExperience,
                ConsultationFee = model.ConsultationFee,
                Qualifications = model.Qualifications,
                Bio = model.Bio,
                DepartmentId = model.DepartmentId
            };
            var result = await _doctorService.CreateAsync(createDto);
            if (result == null)
            {
                ModelState.AddModelError("Email", "Email already exists or registration failed.");
                ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _departmentService.GetAllAsync(), "Id", "Name");
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred.");
            return View(model);
        }
    }

    // show edit form
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();
        var model = new DoctorEditViewModel
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Email = doctor.Email,
            PhoneNumber = doctor.PhoneNumber,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            YearsOfExperience = doctor.YearsOfExperience,
            ConsultationFee = doctor.ConsultationFee,
            Qualifications = doctor.Qualifications,
            Bio = doctor.Bio,
            IsAvailable = doctor.IsAvailable,
            DepartmentId = doctor.DepartmentId
        };
        ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _departmentService.GetAllAsync(), "Id", "Name", doctor.DepartmentId);
        return View(model);
    }

    // save doctor edits
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DoctorEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        try
        {
            var updateDto = new BL.DTOs.Doctor.DoctorUpdateDto
            {
                PhoneNumber = model.PhoneNumber,
                Specialization = model.Specialization,
                YearsOfExperience = model.YearsOfExperience,
                ConsultationFee = model.ConsultationFee,
                Qualifications = model.Qualifications,
                Bio = model.Bio,
                IsAvailable = model.IsAvailable,
                DepartmentId = model.DepartmentId
            };
            var result = await _doctorService.UpdateAsync(id, updateDto);
            if (result == null)
            {
                return NotFound();
            }
            TempData["SuccessMessage"] = "Doctor updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor {Id}", id);
            ModelState.AddModelError("", "An unexpected error occurred.");
            ViewBag.Departments = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _departmentService.GetAllAsync(), "Id", "Name", model.DepartmentId);
            return View(model);
        }
    }

    // delete doctor
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = "Doctor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the doctor.";
            return RedirectToAction(nameof(Index));
        }
    }

    // manage working hours (GET)
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> ManageWorkingHours()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null)
            return NotFound("Doctor profile not found");
        var hours = await _workingHoursService.GetWorkingHoursAsync(doctor.Id);
        var model = new WorkingHoursViewModel
        {
            DoctorId = doctor.Id,
            DoctorName = doctor.FullName,
            WorkingDays = hours.Select(h => new WorkingDayViewModel
            {
                Id = h.Id,
                DayOfWeek = h.DayOfWeek,
                IsWorkingDay = h.IsWorkingDay,
                StartTime = h.StartTime,
                EndTime = h.EndTime
            }).OrderBy(h => h.DayOfWeek == 0 ? 7 : h.DayOfWeek).ToList()
        };
        return View(model);
    }

    // save working hours (POST)
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageWorkingHours(WorkingHoursViewModel model)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null || doctor.Id != model.DoctorId)
        {
            return Forbid();
        }
        var dtos = model.WorkingDays.Select(d => new BL.DTOs.Doctor.WorkingHoursDto
        {
            Id = d.Id,
            DoctorId = model.DoctorId,
            DayOfWeek = d.DayOfWeek,
            IsWorkingDay = d.IsWorkingDay,
            StartTime = d.StartTime,
            EndTime = d.EndTime
        }).ToList();
        await _workingHoursService.UpdateWorkingHoursAsync(model.DoctorId, dtos);
        TempData["SuccessMessage"] = "Working hours updated successfully.";
        return RedirectToAction(nameof(ManageWorkingHours));
    }
}