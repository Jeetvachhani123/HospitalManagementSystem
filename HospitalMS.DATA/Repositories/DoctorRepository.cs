using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HospitalMS.DATA.Repositories;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Doctor> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
    void Update(Doctor doctor);
    void Delete(Doctor doctor);
    Task<bool> LicenseNumberExistsAsync(string licenseNumber);
    Task<int> CountAsync(Expression<Func<Doctor, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchTerm, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
}

public class DoctorRepository : IDoctorRepository
{
    private readonly HospitalDbContext _context;
    public DoctorRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountAsync(Expression<Func<Doctor, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Doctors.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
           .AsNoTracking()
           .Include(d => d.User)
           .Include(d => d.Department)
           .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

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

    public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.Specialization.Contains(specialization))
            .ToListAsync(cancellationToken);
    }

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

    public async Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(doctor, cancellationToken);
        return doctor;
    }

    public void Update(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
    }

    public void Delete(Doctor doctor)
    {
        doctor.IsDeleted = true;
        doctor.DeletedAt = DateTime.UtcNow;
        _context.Doctors.Update(doctor);
    }

    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
    {
        return await _context.Doctors.AnyAsync(d => d.LicenseNumber == licenseNumber);
    }

    public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchTerm, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerQuery = searchTerm.ToLower();
            query = query.Where(d => d.User.FirstName.ToLower().Contains(lowerQuery) || d.User.LastName.ToLower().Contains(lowerQuery) || d.Specialization.ToLower().Contains(lowerQuery) || d.LicenseNumber.ToLower().Contains(lowerQuery));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}