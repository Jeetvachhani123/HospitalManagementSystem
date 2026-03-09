using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.MedicalRecord;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<MedicalRecordsController> _logger;
    public MedicalRecordsController(IMedicalRecordService medicalRecordService, IAppointmentService appointmentService, ILogger<MedicalRecordsController> logger)
    {
        _medicalRecordService = medicalRecordService;
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetByPatientId(int patientId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null || patient.Id != patientId)
            {
                _logger.LogWarning("Patient {UserId} attempted to access records of patient {PatientId}", userId, patientId);
                return Forbid();
            }
        }
        var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
        _logger.LogInformation("Medical records accessed for patient {PatientId} by user {UserId}", patientId, userId);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("my-records")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetMyRecords()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(ApiResponse<IEnumerable<MedicalRecordDto>>.ErrorResponse("Patient profile not found"));
        }
        var records = await _medicalRecordService.GetByPatientIdAsync(patient.Id);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetByDoctorId(int doctorId)
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
        var records = await _medicalRecordService.GetByDoctorIdAsync(doctorId);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("my-created-records")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetMyCreatedRecords()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<IEnumerable<MedicalRecordDto>>.ErrorResponse("Doctor profile not found"));
        }
        var records = await _medicalRecordService.GetByDoctorIdAsync(doctor.Id);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> GetById(int id)
    {
        var record = await _medicalRecordService.GetByIdAsync(id);
        if (record == null)
        {
            return NotFound(ApiResponse<MedicalRecordDto>.ErrorResponse("Medical record not found"));
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Patient")
        {
            var patient = await _appointmentService.GetPatientByUserIdAsync(userId);
            if (patient == null || record.PatientId != patient.Id)
            {
                _logger.LogWarning("Patient {UserId} attempted to access medical record {RecordId}", userId, id);
                return Forbid();
            }
        }
        else if (role == "Doctor")
        {
            var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
            if (doctor == null || record.DoctorId != doctor.Id)
            {
            }
        }
        _logger.LogInformation("Medical record {RecordId} accessed by user {UserId}", id, userId);
        return Ok(ApiResponse<MedicalRecordDto>.SuccessResponse(record));
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> Create([FromBody] MedicalRecordCreateDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var doctor = await _appointmentService.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResponse("Doctor profile not found"));
        }
        dto.DoctorId = doctor.Id;
        var record = await _medicalRecordService.CreateAsync(dto);
        if (record == null)
        {
            return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResponse("Failed to create medical record"));
        }
        _logger.LogInformation("Medical record created by doctor {DoctorId} for patient {PatientId}", doctor.Id, dto.PatientId);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, ApiResponse<MedicalRecordDto>.SuccessResponse(record, "Medical record created successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var result = await _medicalRecordService.DeleteAsync(id, userId);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Medical record not found or you don't have permission to delete it"));
        }
        _logger.LogInformation("Medical record {RecordId} deleted by user {UserId}", id, userId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Medical record deleted successfully"));
    }
}