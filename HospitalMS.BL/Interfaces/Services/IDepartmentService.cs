using HospitalMS.BL.DTOs.Department;

namespace HospitalMS.BL.Interfaces.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();

    Task<DepartmentDto?> GetByIdAsync(int id);

    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);

    Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);

    Task<bool> DeleteAsync(int id);
}