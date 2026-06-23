using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HospitalMS.DATA.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);
    Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Update(Patient patient);
    void Delete(Patient patient);
    Task<(IEnumerable<Patient> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<Patient, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
}

public class PatientRepository : IPatientRepository
{
    private readonly HospitalDbContext _context;
    public PatientRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<int> CountAsync(Expression<Func<Patient, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Patients.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await _context.Patients.AddAsync(patient, cancellationToken);
        return patient;
    }

    public void Update(Patient patient)
    {
        _context.Patients.Update(patient);
    }

    public void Delete(Patient patient)
    {
        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;
        _context.Patients.Update(patient);
    }

    public async Task<(IEnumerable<Patient> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Patients
            .Include(p => p.User)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.User.FirstName.Contains(searchTerm) || p.User.LastName.Contains(searchTerm) || p.User.Email.Contains(searchTerm) || (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Patients
            .Include(p => p.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerQuery = searchTerm.ToLower();
            query = query.Where(p => p.User.FirstName.ToLower().Contains(lowerQuery) || p.User.LastName.ToLower().Contains(lowerQuery) || p.User.Email.ToLower().Contains(lowerQuery) || (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(searchTerm)));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}