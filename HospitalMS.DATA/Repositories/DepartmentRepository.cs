using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly HospitalDbContext _context;
    public DepartmentRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Doctors)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.Doctors)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
    }

    public void Update(Department department)
    {
        _context.Departments.Update(department);
    }

    public void Delete(Department department)
    {
        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
    }
}