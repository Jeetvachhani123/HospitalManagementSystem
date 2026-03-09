using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class AppointmentStatusHistoryRepository : IAppointmentStatusHistoryRepository
{
    private readonly HospitalDbContext _context;
    public AppointmentStatusHistoryRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get history by id
    public async Task<AppointmentStatusHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AppointmentStatusHistories
            .Include(h => h.Appointment)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    // get history by appointment
    public async Task<IEnumerable<AppointmentStatusHistory>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return await _context.AppointmentStatusHistories
            .Where(h => h.AppointmentId == appointmentId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    // add status history
    public async Task AddAsync(AppointmentStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.AppointmentStatusHistories.AddAsync(history, cancellationToken);
    }

    // update status history
    public void Update(AppointmentStatusHistory history)
    {
        _context.AppointmentStatusHistories.Update(history);
    }

    // delete status history
    public void Delete(AppointmentStatusHistory history)
    {
        _context.AppointmentStatusHistories.Remove(history);
    }
}