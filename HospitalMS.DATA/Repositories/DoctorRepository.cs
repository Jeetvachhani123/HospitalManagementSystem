using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly HospitalDbContext _context;
    public DoctorRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get doctor by id
    public async Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
           .AsNoTracking()
           .Include(d => d.User)
           .Include(d => d.Department)
           .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    // get doctor by user id
    public async Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    // get all doctors
    public async Task<IEnumerable<Doctor>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    // get available doctors
    public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.IsAvailable)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    // get by specialization
    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.Specialization.Contains(specialization))
            .ToListAsync(cancellationToken);
    }

    // search doctors
    public async Task<(IEnumerable<Doctor> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d => d.User.FirstName.Contains(searchTerm) || d.User.LastName.Contains(searchTerm) || d.Specialization.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }

    // add doctor
    public async Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(doctor, cancellationToken);
        return doctor;
    }

    // update doctor
    public void Update(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
    }

    // soft delete doctor
    public void Delete(Doctor doctor)
    {
        doctor.IsDeleted = true;
        doctor.DeletedAt = DateTime.UtcNow;
        _context.Doctors.Update(doctor);
    }

    // check license exists
    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
    {
        return await _context.Doctors.AnyAsync(d => d.LicenseNumber == licenseNumber);
    }
}