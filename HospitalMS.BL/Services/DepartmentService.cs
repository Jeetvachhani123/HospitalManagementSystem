using AutoMapper;
using HospitalMS.BL.DTOs.Department;
using HospitalMS.DATA.UnitOfWork;
using HospitalMS.Models.Entities;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task<bool> DeleteAsync(int id);
}

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DepartmentService> _logger;
    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DepartmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();

        return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return null;

        _mapper.Map(dto, department);
        department.Id = id;
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return false;

        if (department.Doctors != null && department.Doctors.Any())
        {
            _logger.LogWarning($"Cannot delete department {id} because it has assigned doctors.");
            return false;
        }

        _unitOfWork.Departments.Delete(department);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}