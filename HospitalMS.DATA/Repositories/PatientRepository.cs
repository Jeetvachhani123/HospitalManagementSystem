using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly HospitalDbContext _context;
    public PatientRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get patient by id
    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    // get patient by user id
    public async Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    // get all patients
    public async Task<IEnumerable<Patient>> GetAllAsync(int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    // add patient
    public async Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await _context.Patients.AddAsync(patient, cancellationToken);
        return patient;
    }

    // update patient
    public void Update(Patient patient)
    {
        _context.Patients.Update(patient);
    }

    // soft delete patient
    public void Delete(Patient patient)
    {
        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;
        _context.Patients.Update(patient);
    }

    // search patients
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
}