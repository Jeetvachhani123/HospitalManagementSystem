using HospitalMS.BL.Common;
using HospitalMS.BL.DTOs.Department;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;
    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(departments));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.ErrorResponse("Department not found"));
        }
        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentDto dto)
    {
        var department = await _departmentService.CreateAsync(dto);
        if (department == null)
        {
            return BadRequest(ApiResponse<DepartmentDto>.ErrorResponse("Failed to create department"));
        }
        _logger.LogInformation("Department {DepartmentName} created", dto.Name);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, ApiResponse<DepartmentDto>.SuccessResponse(department, "Department created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _departmentService.UpdateAsync(id, dto);
        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.ErrorResponse("Department not found"));
        }
        _logger.LogInformation("Department {DepartmentId} updated", id);
        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Department updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Department not found"));
        }
        _logger.LogInformation("Department {DepartmentId} deleted", id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully"));
    }
}