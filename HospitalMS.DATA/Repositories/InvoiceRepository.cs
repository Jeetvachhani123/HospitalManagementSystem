using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly HospitalDbContext _context;
    public InvoiceRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get invoice by id
    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    // get all invoices
    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    // get patient invoices
    public async Task<IEnumerable<Invoice>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    // get invoice by appointment
    public async Task<Invoice?> GetByAppointmentIdAsync(int appointmentId)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
    }

    // get pending invoices
    public async Task<IEnumerable<Invoice>> GetPendingByPatientIdAsync(int patientId)
    {
        return await _context.Invoices
            .Include(i => i.Patient)
                .ThenInclude(p => p.User)
            .Include(i => i.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Where(i => i.PatientId == patientId && !i.IsPaid)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    // add invoice
    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
    }

    // update invoice
    public void Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
    }

    // soft delete invoice
    public void Delete(Invoice invoice)
    {
        invoice.IsDeleted = true;
        invoice.DeletedAt = DateTime.UtcNow;
        _context.Invoices.Update(invoice);
    }
}