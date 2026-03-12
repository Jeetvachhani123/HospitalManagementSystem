using HospitalMS.BL.Interfaces;
using HospitalMS.BL.Interfaces.Services;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BL.Services;

public class BillingService : IBillingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillingService> _logger;
    public BillingService(IUnitOfWork unitOfWork, ILogger<BillingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // get invoice by id
    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _unitOfWork.Invoices.GetByIdAsync(id);
    }

    // get invoice by appointment
    public async Task<Invoice?> GetInvoiceByAppointmentIdAsync(int appointmentId)
    {
        return await _unitOfWork.Invoices.GetByAppointmentIdAsync(appointmentId);
    }

    // get all invoices
    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _unitOfWork.Invoices.GetAllAsync();
    }

    // get patient invoices
    public async Task<IEnumerable<Invoice>> GetPatientInvoicesAsync(int patientId)
    {
        return await _unitOfWork.Invoices.GetByPatientIdAsync(patientId);
    }

    // get pending invoices
    public async Task<IEnumerable<Invoice>> GetPendingInvoicesAsync(int patientId)
    {
        return await _unitOfWork.Invoices.GetPendingByPatientIdAsync(patientId);
    }

    // generate invoice
    public async Task<Invoice> GenerateInvoiceAsync(int appointmentId, decimal amount, DateTime dueDate)
    {
        var existingInvoice = await _unitOfWork.Invoices.GetByAppointmentIdAsync(appointmentId);
        if (existingInvoice != null)
            throw new InvalidOperationException($"Invoice already exists for appointment {appointmentId}");
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment == null)
            throw new InvalidOperationException($"Appointment {appointmentId} not found");
        var invoice = new Invoice { AppointmentId = appointmentId, PatientId = appointment.PatientId, Amount = amount, IssueDate = DateTime.UtcNow, DueDate = dueDate, IsPaid = false };
        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Invoice {InvoiceId} generated for Appointment {AppointmentId}", invoice.Id, appointmentId);
        return invoice;
    }

    // process payment
    public async Task<bool> ProcessPaymentAsync(int invoiceId, string paymentMethod)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null) return false;
        if (invoice.IsPaid)
        {
            _logger.LogWarning("Attempted to pay already paid invoice {InvoiceId}", invoiceId);
            return false;
        }
        invoice.IsPaid = true;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.PaymentMethod = paymentMethod;
        try
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Invoice {InvoiceId} paid via {PaymentMethod}", invoiceId, paymentMethod);
            return true;
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            _logger.LogWarning("Concurrency conflict while processing payment for invoice {InvoiceId}", invoiceId);
            throw new HospitalMS.BL.Exceptions.ConcurrencyException("Invoice", invoiceId);
        }
    }

    // cancel invoice
    public async Task<bool> CancelInvoiceAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null) return false;
        if (invoice.IsPaid)
        {
            _logger.LogWarning("Cannot cancel paid invoice {InvoiceId}", invoiceId);
            return false;
        }
        _unitOfWork.Invoices.Delete(invoice);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Invoice {InvoiceId} cancelled", invoiceId);
        return true;
    }
}