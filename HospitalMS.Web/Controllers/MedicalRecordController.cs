using HospitalMS.BL.DTOs.MedicalRecord;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.Web.Controllers
{
    [Authorize]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MedicalRecordController> _logger;
        public MedicalRecordController(IMedicalRecordService medicalRecordService, IPatientService patientService, IDoctorService doctorService, IWebHostEnvironment environment, ILogger<MedicalRecordController> logger)
        {
            _medicalRecordService = medicalRecordService;
            _patientService = patientService;
            _doctorService = doctorService;
            _environment = environment;
            _logger = logger;
        }

        // list medical records
        public async Task<IActionResult> Index(int? patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Patient")
            {
                var currentPatient = await _patientService.GetByUserIdAsync(userId);
                if (currentPatient == null) return NotFound("Patient profile not found");
                if (!patientId.HasValue || patientId.Value != currentPatient.Id)
                {
                    patientId = currentPatient.Id;
                }
            }
            if (!patientId.HasValue)
            {
                if (role == "Admin" || role == "Doctor")
                {
                    ViewBag.PatientName = "Select a Patient";
                    ViewBag.PatientId = 0;
                    var patients = await _patientService.GetAllAsync();
                    ViewBag.Patients = patients.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.FullName} (ID: {p.Id})"
                    }).ToList();
                    return View(new List<MedicalRecordDisplayViewModel>());
                }
                return BadRequest("Patient ID is required for medical records.");
            }
            var records = await _medicalRecordService.GetByPatientIdAsync(patientId.Value);
            var patient = await _patientService.GetByIdAsync(patientId.Value);
            ViewBag.PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown Patient";
            ViewBag.PatientId = patientId.Value;
            var viewModels = records.Select(r => new MedicalRecordDisplayViewModel
            {
                Id = r.Id,
                PatientId = r.PatientId,
                PatientName = r.PatientName,
                DoctorName = r.DoctorName,
                RecordDate = r.RecordDate,
                RecordType = r.RecordType,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                Notes = r.Notes,
                AttachmentPath = r.AttachmentPath,
                CreatedBy = r.CreatedBy,
                UpdatedBy = r.UpdatedBy
            }).ToList();
            return View(viewModels);
        }

        // show create form
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create(int? patientId)
        {
            if (!patientId.HasValue)
            {
                TempData["InfoMessage"] = "Please select a patient to add a medical record.";
                return RedirectToAction("Index", "Patient");
            }
            var patient = await _patientService.GetByIdAsync(patientId.Value);
            if (patient == null) return NotFound();
            var viewModel = new CreateMedicalRecordViewModel
            {
                PatientId = patientId.Value,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                RecordDate = DateTime.Today
            };
            return View(viewModel);
        }

        // save new record
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMedicalRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string? attachmentPath = null;
                if (viewModel.Attachment != null && viewModel.Attachment.Length > 0)
                {
                    try
                    {
                        var extension = Path.GetExtension(viewModel.Attachment.FileName).ToLowerInvariant();
                        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("Attachment", "Invalid file type. Only PDF and images are allowed.");
                            return View(viewModel);
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        // Store outside of wwwroot for security
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "medical_records", viewModel.PatientId.ToString());
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.Attachment.CopyToAsync(stream);
                        }
                        // AttachmentPath references an internal ID/path, need a dedicated endpoint to download
                        attachmentPath = $"/medicalrecords/download/{viewModel.PatientId}/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file");
                        ModelState.AddModelError("Attachment", "File upload failed. Please try again.");
                        return View(viewModel);
                    }
                }
                int? doctorId = null;
                if (User.IsInRole("Doctor"))
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var doctor = await _doctorService.GetByUserIdAsync(userId);
                    doctorId = doctor?.Id;
                }
                var dto = new MedicalRecordCreateDto
                {
                    PatientId = viewModel.PatientId,
                    DoctorId = doctorId,
                    RecordDate = viewModel.RecordDate,
                    RecordType = viewModel.RecordType,
                    Diagnosis = viewModel.Diagnosis,
                    Prescription = viewModel.Prescription,
                    Notes = viewModel.Notes,
                    AttachmentPath = attachmentPath
                };
                await _medicalRecordService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Medical record added successfully.";
                return RedirectToAction(nameof(Index), new { patientId = viewModel.PatientId });
            }
            return View(viewModel);
        }

        // delete medical record
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _medicalRecordService.DeleteAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete record. You may not have permission.";
            }
            return RedirectToAction(nameof(Index), new { patientId });
        }
    }
}