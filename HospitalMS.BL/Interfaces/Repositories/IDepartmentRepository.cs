using HospitalMS.Models.Entities;

namespace HospitalMS.BL.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);

    Task<IEnumerable<Department>> GetAllAsync();

    Task AddAsync(Department department);

    void Update(Department department);

    void Delete(Department department);
}