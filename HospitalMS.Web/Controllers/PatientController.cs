using HospitalMS.BL.DTOs.Patient;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.BL.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly ISearchService _searchService;
        private readonly ILogger<PatientController> _logger;
        public PatientController(IPatientService patientService, IAppointmentService appointmentService, ISearchService searchService, ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _searchService = searchService;
            _logger = logger;
        }

        // patient dashboard
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var patient = await _patientService.GetByUserIdAsync(userId);
                if (patient == null) return NotFound("Patient profile not found");
                var appointments = await _appointmentService.GetByPatientIdAsync(patient.Id);
                var appointmentList = appointments.ToList();
                var model = new PatientDashboardViewModel
                {
                    Patient = new PatientViewModel
                    {
                        Id = patient.Id,
                        FullName = patient.FullName,
                        Email = patient.Email,
                        DateOfBirth = patient.DateOfBirth,
                        BloodGroup = patient.BloodGroup,
                        Gender = patient.Gender,
                        PhoneNumber = patient.PhoneNumber
                    },
                    UpcomingAppointmentsCount = appointmentList.Count(a => (a.Status == "Scheduled" || a.Status == "Confirmed") && a.ApprovalStatus == "Approved"),
                    CompletedAppointmentsCount = appointmentList.Count(a => a.Status == "Completed"),
                    CancelledAppointmentsCount = appointmentList.Count(a => a.Status == "Cancelled" || a.Status == "NoShow" || a.ApprovalStatus == "Rejected"),
                    PendingApprovalsCount = appointmentList.Count(a => a.ApprovalStatus == "Pending" && a.Status == "Scheduled"),
                    RecentAppointments = appointmentList
                        .Where(a => a.AppointmentDate >= DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .Take(5)
                        .Select(a => new AppointmentViewModel
                        {
                            Id = a.Id,
                            DoctorName = a.DoctorName,
                            Specialization = a.DoctorSpecialization,
                            AppointmentDate = a.AppointmentDate,
                            StartTime = a.StartTime,
                            Status = a.Status,
                            StatusEnum = a.StatusEnum,
                            ApprovalStatus = a.ApprovalStatus,
                            ApprovalStatusEnum = a.ApprovalStatusEnum
                        })
                        .Concat(appointmentList
                            .Where(a => a.AppointmentDate < DateTime.Today)
                            .OrderByDescending(a => a.AppointmentDate)
                            .Take(5)
                            .Select(a => new AppointmentViewModel
                            {
                                Id = a.Id,
                                DoctorName = a.DoctorName,
                                Specialization = a.DoctorSpecialization,
                                AppointmentDate = a.AppointmentDate,
                                StartTime = a.StartTime,
                                Status = a.Status,
                                StatusEnum = a.StatusEnum,
                                ApprovalStatus = a.ApprovalStatus,
                                ApprovalStatusEnum = a.ApprovalStatusEnum
                            }))
                        .ToList()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient dashboard");
                return View("Error");
            }
        }

        // list patients
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index(string? searchQuery, int page = 1, int pageSize = 10)
        {
            try
            {
                IEnumerable<PatientResponseDto> patients;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var searchResults = await _searchService.SearchPatientsAsync(searchQuery);
                    patients = searchResults.Select(s => new PatientResponseDto
                    {
                        Id = s.Id,
                        FullName = s.Name,
                        Email = s.Email,
                        PhoneNumber = s.PhoneNumber,
                        DateOfBirth = DateTime.MinValue,
                        Age = 0,
                        BloodGroup = "",
                        Gender = "",
                        MedicalHistory = "",
                        Allergies = "",
                        EmergencyContact = ""
                    });
                }
                else
                {
                    patients = await _patientService.GetAllAsync();
                }
                var pagedResult = PaginationService.Create(patients, page, pageSize);
                var model = new PatientListViewModel
                {
                    Patients = pagedResult.Items.Select(p => new PatientViewModel
                    {
                        Id = p.Id,
                        FullName = p.FullName,
                        Email = p.Email,
                        DateOfBirth = p.DateOfBirth,
                        Age = p.Age,
                        BloodGroup = p.BloodGroup,
                        Gender = p.Gender,
                        PhoneNumber = p.PhoneNumber
                    }).ToList(),
                    CurrentPage = pagedResult.PageNumber,
                    TotalPages = pagedResult.TotalPages,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    SearchQuery = searchQuery
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patients");
                return View("Error");
            }
        }

        // patient details
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null) return NotFound();
            if (User.IsInRole("Patient") && patient.UserId != userId)
            {
                return Forbid();
            }
            var viewModel = new PatientViewModel
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                BloodGroup = patient.BloodGroup,
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                MedicalHistory = patient.MedicalHistory,
                Allergies = patient.Allergies,
                EmergencyContact = patient.EmergencyContact,
                CreatedBy = patient.CreatedBy,
                UpdatedBy = patient.UpdatedBy
            };
            return View(viewModel);
        }

        // show create patient form
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // save new patient
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var createDto = new PatientCreateDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    BloodGroup = model.BloodGroup,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    EmergencyContact = ""
                };
                var result = await _patientService.CreateAsync(createDto);
                if (result == null)
                {
                    ModelState.AddModelError("Email", "Email already exists or registration failed.");
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(model);
            }
        }

        // show edit patient form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null) return NotFound();
            var model = new PatientEditViewModel
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Email = patient.Email,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber,
                BloodGroup = patient.BloodGroup,
                Gender = patient.Gender,
                EmergencyContact = patient.EmergencyContact,
                MedicalHistory = patient.MedicalHistory,
                Allergies = patient.Allergies
            };
            return View(model);
        }

        // save patient edits
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            try
            {
                var updateDto = new PatientUpdateDto
                {
                    PhoneNumber = model.PhoneNumber,
                    BloodGroup = model.BloodGroup,
                    Gender = model.Gender,
                    EmergencyContact = model.EmergencyContact,
                    MedicalHistory = model.MedicalHistory,
                    Allergies = model.Allergies
                };
                var result = await _patientService.UpdateAsync(id, updateDto);
                if (result == null)
                {
                    return NotFound();
                }
                TempData["SuccessMessage"] = "Patient updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {Id}", id);
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View(model);
            }
        }

        // delete patient
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _patientService.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Patient deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the patient.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}