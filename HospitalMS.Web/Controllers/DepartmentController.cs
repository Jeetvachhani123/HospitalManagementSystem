using HospitalMS.BL.DTOs.Department;
using HospitalMS.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Web.Controllers;

[Authorize]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // list departments
    public async Task<IActionResult> Index()
    {
        var departments = await _departmentService.GetAllAsync();
        return View(departments);
    }

    // department details
    public async Task<IActionResult> Details(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) 
            return NotFound();
        
        return View(department);
    }

    // show create form
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // save new department
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        if (ModelState.IsValid)
        {
            await _departmentService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(dto);
    }

    // show edit form
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) 
            return NotFound();
        
        var updateDto = new UpdateDepartmentDto { Id = department.Id, Name = department.Name, Description = department.Description, Location = department.Location, IsActive = department.IsActive };
        return View(updateDto);
    }

    // save department edits
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDepartmentDto dto)
    {
        if (id != dto.Id) 
            return BadRequest();
        
        if (ModelState.IsValid)
        {
            await _departmentService.UpdateAsync(id, dto);
            TempData["SuccessMessage"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(dto);
    }

    // delete department
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _departmentService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Department deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not delete department. It may have assigned doctors.";
        }
        
        return RedirectToAction(nameof(Index));
    }
}