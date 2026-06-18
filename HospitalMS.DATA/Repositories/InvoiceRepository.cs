using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetPendingByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    void Update(Invoice invoice);
    void Delete(Invoice invoice);
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly HospitalDbContext _context;
    public InvoiceRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetPendingByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Where(i => i.PatientId == patientId && !i.IsPaid)
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
    }

    public void Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
    }

    public void Delete(Invoice invoice)
    {
        invoice.IsDeleted = true;
        invoice.DeletedAt = DateTime.UtcNow;
        _context.Invoices.Update(invoice);
    }
}