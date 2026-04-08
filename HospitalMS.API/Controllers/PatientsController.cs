using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Patient;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PatientResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var patients = await _patientService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<PatientResponseDto>>.SuccessResponse(patients));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PatientResponseDto>>> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null)
        {
            return NotFound(ApiResponse<PatientResponseDto>.ErrorResponse(Constants.Messages.PatientNotFound));
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Patient" && patient.UserId != userId)
        {
            return Forbid();
        }

        return Ok(ApiResponse<PatientResponseDto>.SuccessResponse(patient));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<PatientResponseDto>>> GetByUserId(int userId)
    {
        var patient = await _patientService.GetByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(ApiResponse<PatientResponseDto>.ErrorResponse(Constants.Messages.PatientNotFound));
        }

        return Ok(ApiResponse<PatientResponseDto>.SuccessResponse(patient));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PatientResponseDto>>> Create([FromBody] PatientCreateDto patientDto)
    {
        var patient = await _patientService.CreateAsync(patientDto);
        if (patient == null)
        {
            return BadRequest(ApiResponse<PatientResponseDto>.ErrorResponse(Constants.Messages.EmailAlreadyExists));
        }

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, ApiResponse<PatientResponseDto>.SuccessResponse(patient, "Patient registered successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _patientService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(Constants.Messages.PatientNotFound));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Patient deleted successfully"));
    }
}