using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class DoctorWorkingHoursRepository : IDoctorWorkingHoursRepository
{
    private readonly HospitalDbContext _context;
    public DoctorWorkingHoursRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorWorkingHours?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorWorkingHours
            .FirstOrDefaultAsync(dwh => dwh.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DoctorWorkingHours>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorWorkingHours
            .AsNoTracking()
            .Where(dwh => dwh.DoctorId == doctorId)
            .OrderBy(dwh => dwh.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task<DoctorWorkingHours?> GetByDoctorIdAndDayAsync(int doctorId, int dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await _context.DoctorWorkingHours
            .FirstOrDefaultAsync(dwh => dwh.DoctorId == doctorId && dwh.DayOfWeek == dayOfWeek, cancellationToken);
    }

    public async Task<DoctorWorkingHours> AddAsync(DoctorWorkingHours workingHours, CancellationToken cancellationToken = default)
    {
        await _context.DoctorWorkingHours.AddAsync(workingHours, cancellationToken);
        return workingHours;
    }

    public void Update(DoctorWorkingHours workingHours)
    {
        _context.DoctorWorkingHours.Update(workingHours);
    }

    public void Delete(DoctorWorkingHours workingHours)
    {
        workingHours.IsDeleted = true;
        workingHours.DeletedAt = DateTime.UtcNow;
        _context.DoctorWorkingHours.Update(workingHours);
    }
}