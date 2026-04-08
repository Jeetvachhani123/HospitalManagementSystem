using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly HospitalDbContext _context;
    public AppointmentRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get appointment by id
    public async Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    // read-only get by id
    public async Task<Appointment?> GetByIdForReadAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    // get all appointments
    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    // get by patient
    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    // get by doctor
    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    // get by date range
    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    // get by status
    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    // get pending approvals
    public async Task<IEnumerable<Appointment>> GetPendingApprovalsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctorId && a.ApprovalStatus == AppointmentApprovalStatus.Pending && a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    // get today's appointments
    public async Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .Where(a => a.AppointmentDate >= todayUtc && a.AppointmentDate < tomorrowUtc && !a.IsDeleted && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    // check time slot conflict
    public async Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentDate.Date && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow && a.ApprovalStatus != AppointmentApprovalStatus.Rejected && ((a.StartTime < endTime && a.EndTime > startTime)));
        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    // get available time slots
    public async Task<IEnumerable<TimeSpan>> GetAvailableSlotsAsync(int doctorId, DateTime date, int slotDurationMinutes = 30, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        if (date.Date < today)
            return new List<TimeSpan>();
        
        var dayOfWeek = (int)date.DayOfWeek;
        var workingHours = await _context.DoctorWorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(dw => dw.DoctorId == doctorId && dw.DayOfWeek == dayOfWeek, cancellationToken);
        if (workingHours == null || workingHours.StartTime >= workingHours.EndTime)
        {
            workingHours = new DoctorWorkingHours
            {
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(21, 0, 0),
                IsWorkingDay = true
            };
        }

        if (!workingHours.IsWorkingDay)
        {
            if (dayOfWeek == 0 || dayOfWeek == 6)
                return new List<TimeSpan>();
            workingHours.IsWorkingDay = true;
            workingHours.StartTime = new TimeSpan(9, 0, 0);
            workingHours.EndTime = new TimeSpan(21, 0, 0);
        }

        var bookedSlots = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status != AppointmentStatus.Cancelled && a.ApprovalStatus != AppointmentApprovalStatus.Rejected)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync(cancellationToken);
        var availableSlots = new List<TimeSpan>();
        var currentTime = workingHours.StartTime;
        var now = DateTime.Now;
        while (currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes)) <= workingHours.EndTime)
        {
            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));
            if (date.Date == today && currentTime < now.TimeOfDay)
            {
                currentTime = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));
                continue;
            }
            var isAvailable = !bookedSlots.Any(b =>
                (currentTime < b.EndTime && slotEndTime > b.StartTime)
            );
            if (isAvailable)
                availableSlots.Add(currentTime);
            currentTime = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));
        }
        if (!availableSlots.Any())
        {
            var fallbackStartTime = new TimeSpan(9, 0, 0);
            var fallbackEndTime = new TimeSpan(17, 0, 0);
            var tempTime = fallbackStartTime;
            while (tempTime.Add(TimeSpan.FromMinutes(slotDurationMinutes)) <= fallbackEndTime)
            {
                if (date.Date > today || tempTime >= now.TimeOfDay)
                    availableSlots.Add(tempTime);
                tempTime = tempTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));
            }
        }

        return availableSlots;
    }

    // add appointment
    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
       
        return appointment;
    }

    // update appointment
    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }

    // soft delete appointment
    public void Delete(Appointment appointment)
    {
        appointment.IsDeleted = true;
        appointment.DeletedAt = DateTime.UtcNow;
        _context.Appointments.Update(appointment);
    }

    // count appointments
    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Appointment, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments.AsNoTracking();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.CountAsync(cancellationToken);
    }

    // get recent appointments
    public async Task<IEnumerable<Appointment>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    // search appointments
    public async Task<(IEnumerable<Appointment> Items, int TotalCount)> SearchAsync(string? searchTerm, int? doctorId, int? patientId, DateTime? fromDate, DateTime? toDate, AppointmentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .AsSplitQuery()
            .AsQueryable();
        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        if (patientId.HasValue)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= toDate.Value.Date);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerQuery = searchTerm.ToLower();
            query = query.Where(a => a.Patient!.User!.FirstName.Contains(searchTerm) || a.Patient!.User!.LastName.Contains(searchTerm) || a.Doctor!.User!.FirstName.Contains(searchTerm) || a.Doctor!.User!.LastName.Contains(searchTerm) || (a.Reason != null && a.Reason.Contains(searchTerm)) || (a.Diagnosis != null && a.Diagnosis.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }
}